using Entities;
using WebApi.ViewModels;

namespace WebApi.Mapping
{
    public class MeetingProfile : AutoMapper.Profile
    {
        public MeetingProfile()
        {
            CreateMap<Meeting, MeetingViewModel>();
            CreateMap<MeetingViewModel, Meeting>();
        }

    }
}
