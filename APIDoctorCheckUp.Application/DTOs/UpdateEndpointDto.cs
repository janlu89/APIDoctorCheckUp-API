using System.ComponentModel.DataAnnotations;

namespace APIDoctorCheckUp.Application.DTOs;

public record UpdateEndpointDto(
    [Required, MaxLength(100)]  string Name,
    [Required, MaxLength(2048), Url] string Url,
    int ExpectedStatusCode      = 200,
    int CheckIntervalSeconds    = 60,
    bool IsActive               = true,
    int ResponseTimeWarningMs   = 1000,
    int ResponseTimeCriticalMs  = 3000,
    int ConsecutiveFailuresDown = 3
);
