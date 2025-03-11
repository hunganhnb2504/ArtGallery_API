using ArtGallery.Helper;

namespace ArtGallery.Service.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(Mailrequest mailrequest);
    }
}
