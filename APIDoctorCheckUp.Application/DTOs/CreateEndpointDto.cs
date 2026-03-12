using System.ComponentModel.DataAnnotations;

namespace APIDoctorCheckUp.Application.DTOs;

public record CreateEndpointDto(
    [Required, MaxLength(100)]  string Name,
    [Required, MaxLength(2048), Url] string Url,
    int ExpectedStatusCode      = 200,
    int CheckIntervalSeconds    = 60,
    int ResponseTimeWarningMs   = 1000,
    int ResponseTimeCriticalMs  = 3000,
    int ConsecutiveFailuresDown = 3
);
