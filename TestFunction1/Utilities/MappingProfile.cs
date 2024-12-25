using AutoMapper;
using TestFunction1.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<BetterTrucksOrder, AmerishipOrder>()
            .ForMember(d => d.trackingNumber, opt => opt.MapFrom(s => s.TrackingNumber))
            .ForPath(d => d.deliveryLocation.contactName, opt => opt.MapFrom(s => s.ContactName))
            .ForPath(d => d.deliveryLocation.companyName, opt => opt.MapFrom(s => s.Company))
            .ForPath(d => d.deliveryLocation.addressLine1, opt => opt.MapFrom(s => s.AddressLine1))
            .ForPath(d => d.deliveryLocation.city, opt => opt.MapFrom(s => s.City))
            .ForPath(d => d.deliveryLocation.state, opt => opt.MapFrom(s => s.State))
            .ForPath(d => d.deliveryLocation.postalCode, opt => opt.MapFrom(s => s.PostalCode))
            .ForPath(d => d.deliveryLocation.email, opt => opt.MapFrom(s => s.Email))
            .ForPath(d => d.deliveryLocation.phone, opt => opt.MapFrom(s => s.Phone))
            .ForPath(d => d.deliveryLocation.country, opt => opt.MapFrom(s => s.Country))
            .ForMember(d => d.deliverySignatureRequired, opt => opt.MapFrom(s => s.SignatureRequired))
            .ForMember(d => d.signatureType, opt => opt.MapFrom(s => s.SignatureType))
            .ForMember(d => d.isAdultSignature, opt => opt.MapFrom(s => s.AdultSignature));
            
    }
}