using AutoMapper;
using ContactManagement.Core.DTOs.Categories;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.DTOs.Auth;
using ContactManagement.Core.Entities;

namespace ContactManagement.Services.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Contact, ContactDto>()
            .ForMember(d => d.Categories, o => o.MapFrom(s => s.ContactCategories.Select(cc => cc.Category)));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ContactCount, o => o.MapFrom(s => s.ContactCategories.Count));
    }
}
