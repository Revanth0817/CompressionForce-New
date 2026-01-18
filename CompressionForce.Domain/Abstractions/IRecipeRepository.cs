using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{


    public interface IRecipeRepository
    {
        Task<List<Recipe>> GetAllAsync();
        Task<Recipe> GetByCodeAsync(string recipeCode);
    }
}
