using System.Collections.Generic;
using env_analysis_project.Models;

namespace env_analysis_project.Services
{
    public interface IPredictionService
    {
        PredictionResult UploadAndPredict(string filePath);
        PredictionResult PredictFromData(IEnumerable<PollutionData> data);
    }
}


