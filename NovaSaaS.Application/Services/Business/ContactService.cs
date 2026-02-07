using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class ContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ContactService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateContactDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
            if (customer == null) throw new ArgumentException("Khách hàng không tồn tại.");

            if (dto.IsPrimary)
            {
                var existingPrimary = await _unitOfWork.Repository<Contact>()
                    .FindAsync(c => c.CustomerId == dto.CustomerId && c.IsPrimary);
                foreach (var c in existingPrimary)
                {
                    c.IsPrimary = false;
                    _unitOfWork.Repository<Contact>().Update(c);
                }
            }

            var contact = new Contact
            {
                CustomerId = dto.CustomerId,
                FullName = dto.FullName,
                JobTitle = dto.JobTitle,
                Email = dto.Email,
                Phone = dto.Phone,
                Notes = dto.Notes,
                IsPrimary = dto.IsPrimary,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Contact>().Add(contact);
            await _unitOfWork.CompleteAsync();
            return contact.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateContactDto dto)
        {
            var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
            if (contact == null) throw new ArgumentException("Liên hệ không tồn tại.");

            if (dto.IsPrimary && !contact.IsPrimary)
            {
                var existingPrimary = await _unitOfWork.Repository<Contact>()
                    .FindAsync(c => c.CustomerId == contact.CustomerId && c.IsPrimary && c.Id != id);
                foreach (var c in existingPrimary)
                {
                    c.IsPrimary = false;
                    _unitOfWork.Repository<Contact>().Update(c);
                }
            }

            contact.FullName = dto.FullName;
            contact.JobTitle = dto.JobTitle;
            contact.Email = dto.Email;
            contact.Phone = dto.Phone;
            contact.Notes = dto.Notes;
            contact.IsPrimary = dto.IsPrimary;
            contact.UpdateAt = DateTime.UtcNow;
            contact.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Contact>().Update(contact);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
            if (contact == null) return false;

            _unitOfWork.Repository<Contact>().Remove(contact);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<List<ContactDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var contacts = await _unitOfWork.Repository<Contact>()
                .FindAsync(c => c.CustomerId == customerId);
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            return contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = customer?.Name ?? "",
                FullName = c.FullName,
                JobTitle = c.JobTitle,
                Email = c.Email,
                Phone = c.Phone,
                Notes = c.Notes,
                IsPrimary = c.IsPrimary,
                CreateAt = c.CreateAt
            }).OrderByDescending(c => c.IsPrimary).ThenBy(c => c.FullName).ToList();
        }

        public async Task<ContactDto?> GetByIdAsync(Guid id)
        {
            var c = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
            if (c == null) return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(c.CustomerId);

            return new ContactDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = customer?.Name ?? "",
                FullName = c.FullName,
                JobTitle = c.JobTitle,
                Email = c.Email,
                Phone = c.Phone,
                Notes = c.Notes,
                IsPrimary = c.IsPrimary,
                CreateAt = c.CreateAt
            };
        }
    }
}
