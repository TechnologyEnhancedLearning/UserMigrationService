using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Pipeline
{
    public class MigrationPipeline : IMigrationPipeline
    {
        private readonly ILearningHubRepository _learningHubRepository;
        private readonly ILegacyRepository _legacyRepository;

        public MigrationPipeline(
            ILearningHubRepository learningHubRepository,
            ILegacyRepository legacyRepository)
        {
            _learningHubRepository = learningHubRepository;
            _legacyRepository = legacyRepository;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _learningHubRepository.TestConnectionAsync();
            await _legacyRepository.TestConnectionAsync();
        }
    }
}
