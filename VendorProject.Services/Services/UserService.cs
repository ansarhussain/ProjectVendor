using AutoMapper;
using VendorProject.Common.DTOs;
using VendorProject.Services.Repositories;

namespace VendorProject.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<PaginatedResponse<UserDto>> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var (users, totalCount) = await _repository.GetAllPaginatedAsync(query, cancellationToken);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            return new PaginatedResponse<UserDto>
            {
                Data = userDtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }
    }
}
