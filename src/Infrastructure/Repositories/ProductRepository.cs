using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        // Construtor para receber o AppDbContext e passar pra base GenericRepository
        public ProductRepository(AppDbContext context) : base(context) {}
    }
}