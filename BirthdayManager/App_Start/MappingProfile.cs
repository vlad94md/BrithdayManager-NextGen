using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using BirthdayManager.Controllers.Api;
using BirthdayManager.Core.Dtos;
using BirthdayManager.Core.Entities;
using BirthdayManager.ViewModels;

namespace BirthdayManager
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.ToLower()))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullname()));

            CreateMap<UserDto, ApplicationUser>();
            CreateMap<ApplicationUser, UserFormViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>();
        }
    }
}