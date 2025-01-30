namespace LMSMVC.Models;

//Model which help tp display the Error Details
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? ErrorMessage { get; set; }

}
