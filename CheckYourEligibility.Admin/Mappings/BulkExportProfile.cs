using AutoMapper;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;
using System.Diagnostics.CodeAnalysis;


namespace CheckYourEligibility.Admin.Mappings
{
    [ExcludeFromCodeCoverage]
    public class BulkExportProfile : Profile
    {
        public BulkExportProfile()
        {
            CreateMap<CheckEligibilityItem, BulkExportBase>()
                .ForMember(d => d.Outcome,
                    opt => opt.MapFrom(s => s.Status.GetFsmStatusDescriptionBulkCheck(s.Tier)));

            CreateMap<CheckEligibilityItem, BulkExport>()
                .ForMember(d => d.Outcome,
                    opt => opt.MapFrom(s => s.Status.GetFsmStatusDescriptionBulkCheck(s.Tier)))
                .ForMember(d => d.EligibilityEndDate,
                    opt => opt.MapFrom(s => s.EligibilityEndDate));
        }
    }
}
