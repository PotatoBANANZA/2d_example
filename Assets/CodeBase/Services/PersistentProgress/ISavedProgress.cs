using CodeBase.Data;

namespace CodeBase.Services.PersistentProgress
{
  public interface ISavedProgress : ISavedProgressReader
  {
    void UpdateProgress(PlayerProgress progress);
  }
}