using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionSharingApp.Services
{
    public class SolutionService
    {
        private readonly ApplicationDbContext _context;
        private readonly NLPService _nlpService;
        private readonly ILogger<SolutionService> _logger;

        public SolutionService(ApplicationDbContext context, NLPService nlpService, ILogger<SolutionService> logger)
        {
            _context = context;
            _nlpService = nlpService;
            _logger = logger;
        }

        public async Task<List<Solution>> GetSolutionsAsync()
        {
            return await _context.Solutions.ToListAsync();
        }

        public async Task<Solution?> GetSolutionByIdAsync(int id)
        {
            return await _context.Solutions.FindAsync(id);
        }

        public async Task CreateSolutionAsync(Solution solution)
        {
            var (keywords, vector) = await _nlpService.GetKeywordsAndVectorAsync(solution.SolutionContent);
            solution.Keywords = string.Join(",", keywords ?? new string[0]); // Null kontrolü ve virgülle ayrılmış dize
            solution.Vector = vector ?? new float[0]; // Null kontrolü ve varsayılan değer

            _logger.LogInformation("Creating solution with Keywords: {Keywords}, Vector: {Vector}", solution.Keywords, vector);

            _context.Solutions.Add(solution);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSolutionAsync(int id, Solution updatedSolution)
        {
            var solution = await _context.Solutions.FirstOrDefaultAsync(x => x.Id == id);
            if (solution != null)
            {
                solution.Title = updatedSolution.Title;
                solution.Author = updatedSolution.Author;
                solution.Category = updatedSolution.Category;
                solution.Problem = updatedSolution.Problem;
                solution.SolutionContent = updatedSolution.SolutionContent;
                solution.Photos = updatedSolution.Photos;
                solution.PhotoDescriptions = updatedSolution.PhotoDescriptions;
                solution.Status = updatedSolution.Status;
                solution.RejectReason = updatedSolution.RejectReason;
                solution.ReturnReason = updatedSolution.ReturnReason;

                var (keywords, vector) = await _nlpService.GetKeywordsAndVectorAsync(solution.SolutionContent);
                solution.Keywords = string.Join(",", keywords ?? new string[0]); // Null kontrolü ve virgülle ayrılmış dize
                solution.Vector = vector ?? new float[0]; // Null kontrolü ve varsayılan değer

                _logger.LogInformation("Updating solution with Keywords: {Keywords}, Vector: {Vector}", solution.Keywords, vector);

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSolutionAsync(int id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution != null)
            {
                _context.Solutions.Remove(solution);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Solution>> SearchSolutionsAsync(string searchText)
        {
            var (keywords, searchVector) = await _nlpService.GetKeywordsAndVectorAsync(searchText);

            var solutions = await _context.Solutions.ToListAsync();
            var scoredSolutions = solutions
                .Where(s => s.Vector != null)
                .Select(s => new
                {
                    Solution = s,
                    Score = CosineSimilarity(s.Vector!, searchVector)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Solution)
                .ToList();

            return scoredSolutions;
        }

        private double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA == null || vectorB == null)
                return 0;

            var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            var magnitudeA = Math.Sqrt(vectorA.Sum(a => a * a));
            var magnitudeB = Math.Sqrt(vectorB.Sum(b => b * b));

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
