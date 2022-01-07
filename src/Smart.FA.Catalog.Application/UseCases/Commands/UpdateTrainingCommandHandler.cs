using Application.SeedWork;
using Application.UseCases.Dto;
using Core.Domain;
using Core.Domain.Interfaces;
using Core.SeedWork;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands;

public class UpdateTrainingCommandHandler : IRequestHandler<UpdateTrainingRequest, UpdateTrainingResponse>
{
    private readonly ILogger<UpdateTrainingCommandHandler> _logger;
    private readonly ITrainingRepository _trainingRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainingCommandHandler(ILogger<UpdateTrainingCommandHandler> logger,
        ITrainingRepository trainingRepository, ITrainerRepository trainerRepository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _trainingRepository = trainingRepository;
        _trainerRepository = trainerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateTrainingResponse> Handle(UpdateTrainingRequest request, CancellationToken cancellationToken)
    {
        UpdateTrainingResponse resp = new();

        try
        {
            var training = await _trainingRepository.GetFullTraGetTrainingAsync(request.TrainingId, cancellationToken);
            training.UpdateDetails(request.Detail.Title, request.Detail.Goal, request.Detail.Methodology,
                Language.Create(request.Detail.Language).Value);
            training.SwitchTrainingTypes(request.Types);
            training.SwitchTargetAudience(request.TargetAudiences);
            training.SwitchSlotNumberType(request.SlotNumberTypes);
            var trainers = await _trainerRepository.GetListAsync(request.TrainerIds, cancellationToken);
            training.EnrollTrainers(trainers);

            _unitOfWork.RegisterDirty(training);
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

public class UpdateTrainingRequest : IRequest<UpdateTrainingResponse>
{
    public int TrainingId { get; set; }
    public TrainingDetailDto? Detail { get; set; }
    public List<TrainingTargetAudience>? TargetAudiences { get; set; }
    public List<TrainingType>? Types { get; set; }
    public List<TrainingSlotNumberType>? SlotNumberTypes { get; set; }
    public List<int> TrainerIds { get; set; }
}

public class UpdateTrainingResponse : ResponseBase
{
    public Training Training { get; set; }
}
