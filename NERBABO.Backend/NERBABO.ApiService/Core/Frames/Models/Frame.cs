using System.Globalization;
using Humanizer;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Frames.Models;

public class Frame : Entity
{
    public string Program { get; set; } = string.Empty;
    public string Intervention { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;


    public static Frame ConvertCreateDtoToEntity(CreateFrameDto frameDto)
    {
        return new Frame
        {
            Program = frameDto.Program,
            Intervention = frameDto.Intervention,
            InterventionType = frameDto.InterventionType,
            Operation = frameDto.Operation,
            OperationType = frameDto.OperationType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Frame ConvertUpdateDtoToEntity(UpdateFrameDto frameDto)
    {
        return new Frame
        {
            Id = frameDto.Id,
            Program = frameDto.Program,
            Intervention = frameDto.Intervention,
            InterventionType = frameDto.InterventionType,
            Operation = frameDto.Operation,
            OperationType = frameDto.OperationType,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static RetrieveFrameDto ConvertEntityToRetrieveDto(Frame frame)
    {
        return new RetrieveFrameDto
        {
            Id = frame.Id,
            Program = frame.Program,
            Intervention = frame.Intervention,
            InterventionType = frame.InterventionType,
            Operation = frame.Operation,
            OperationType = frame.OperationType,
            CreatedAt = frame.CreatedAt.Humanize(culture: new CultureInfo("pt-PT"))
        };
    }
}
