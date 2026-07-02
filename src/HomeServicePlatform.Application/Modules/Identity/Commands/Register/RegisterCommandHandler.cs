using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HomeServicePlatform.Application.Modules.Identity.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<long>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(IGenericRepository<User> userRepo, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepo = userRepo; _passwordHasher = passwordHasher; _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<long>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.Trim().ToLower(); // Đồng bộ chữ thường
            var phoneTrimmed = request.Phone.Trim();

            var existingByEmail = (await _userRepo.FindAsync(u => !u.IsDeleted && u.Email == emailLower)).FirstOrDefault();
            if (existingByEmail != null) throw new BadRequestException("Email này đã được sử dụng.");

            var existingByPhone = (await _userRepo.FindAsync(u => !u.IsDeleted && u.Phone == phoneTrimmed)).FirstOrDefault();
            if (existingByPhone != null) throw new BadRequestException("Số điện thoại này đã được sử dụng.");

            var user = new User
            {
                Email = emailLower,
                Phone = phoneTrimmed,
                FullName = request.FullName.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Status = 1,
                UserRoles = new List<UserRole> { new() { RoleId = request.RoleId } }
                // TUYỆT ĐỐI KHÔNG gán CreatedAt hay UpdatedAt ở đây
            };

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<long>.Success(user.UserId, "Đăng ký tài khoản thành công", 201);
        }
    }
}
