using Application.SeedWork;
using Core.Domain;
using Core.Domain.Dto;
using Core.Domain.Enumerations;
using Core.Domain.Interfaces;
using Core.SeedWork;
using Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands;

public class CreateTrainingCommandHandler : IRequestHandler<CreateTrainingRequest, CreateTrainingResponse>
{
    private readonly ILogger<CreateTrainingCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IMailService _mailService;

    public CreateTrainingCommandHandler(ILogger<CreateTrainingCommandHandler> logger
        , IUnitOfWork unitOfWork
        , ITrainerRepository trainerRepository
        , IMailService mailService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _trainerRepository = trainerRepository;
        _mailService = mailService;
    }

    public async Task<CreateTrainingResponse> Handle(CreateTrainingRequest request, CancellationToken cancellationToken)
    {
        CreateTrainingResponse resp = new();
        try
        {
            var trainer = await _trainerRepository.FindAsync(request.TrainerId, cancellationToken);
            var training = new Training(trainer, request.Types, request.SlotNumberTypes, request.TargetAudiences);
            training.AddDetails(request.Detail.Title, request.Detail.Goal, request.Detail.Methodology,
                Language.Create(request.Detail.Language).Value);
            training.Validate(_mailService);

            _unitOfWork.RegisterNew(training);
            _unitOfWork.Commit();

            resp.Training = training;
            resp.SetSuccess();
        }
        catch (Exception e)
        {
            _logger.LogError(e.StackTrace);
            throw;
        }

        return resp;
    }
}

public class CreateTrainingRequest : IRequest<CreateTrainingResponse>
{
    public int TrainerId { get; set; }
    public TrainingDetailDto? Detail { get; set; }
    public List<TrainingTargetAudience>? TargetAudiences { get; set; }
    public List<TrainingType>? Types { get; set; }
    public List<TrainingSlotNumberType>? SlotNumberTypes { get; set; }
}

public class CreateTrainingResponse : ResponseBase
{
    public Training? Training { get; set; }
}
