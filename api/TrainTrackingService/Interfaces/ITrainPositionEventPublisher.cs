using TrainTrackingService.Models;

public interface ITrainPositionEventPublisher
{
    Task PublishTrainPosition(TrainPosition position);
}
