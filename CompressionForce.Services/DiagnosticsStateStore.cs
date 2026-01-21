using CompressionForce.Domain.ViewModels;

namespace CompressionForce.Services;

public class DiagnosticsStateStore
{
    public DiagnosticsFixedVM? Current { get; private set; }

    public void Update(DiagnosticsFixedVM data)
    {
        Current = data;
    }
}
