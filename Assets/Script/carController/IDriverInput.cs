public interface IDriverInput
{
    float Throttle { get; }  // [0,1]
    float Steer { get; }  // [-1,1]
    bool Brake { get; }  // ��ɲ��
    bool Handbrake { get; }  // ��ɲ/Ư��
}

