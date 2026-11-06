using System.ComponentModel.DataAnnotations;

namespace ConversationalCVBuilder.Api.Contracts.Cv;

public sealed record GenerateCvRequest([param: Required] Guid CvProfileId);
