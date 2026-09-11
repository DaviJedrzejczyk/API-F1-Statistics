using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using WebApi.ViewModels;

namespace WebApi.AutoMapperConfig
{
    public class MapConfig : Profile
    {
        public MapConfig()
        {
            //Web API
            CreateMap<Meeting, MeetingViewModel>().ReverseMap();
            CreateMap<Session, SessionViewModel>().ReverseMap();
            CreateMap<DriverListViewModel, Driver>().ReverseMap();
            CreateMap<SessionResult, SessionResultListViewModel>().ReverseMap();

            //External Api / Entities
            CreateMap<CarDataDto, CarData>().ReverseMap();
            CreateMap<Meeting, MeetingDto>().ReverseMap();
            CreateMap<SessionDto, Session>().ReverseMap();
            CreateMap<DriverDto,  Driver>().ReverseMap();
            CreateMap<SessionResult, SessionResultDto>().ReverseMap();
            CreateMap<Pit, PitDto>().ReverseMap();
            CreateMap<Overtake, OvertakeDto>().ReverseMap();
            CreateMap<RaceControl, RaceControlDto>().ReverseMap();
            CreateMap<StintListDTO, Stint>().ReverseMap();
        }
    }
}
