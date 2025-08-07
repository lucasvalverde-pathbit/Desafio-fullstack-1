using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        // Construtor para receber o AppDbContext e passar pra base GenericRepository
        public OrderRepository(AppDbContext context) : base(context) {}

        // Método implementado para fazer o get de todos os pedidos
        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .ToListAsync();
        }

        // Método implementado para encontrar um pedido pelo Id do cliente
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .ToListAsync();
        }

        // Método implementado para adicionar produtos em de um pedido em OrderProduct
        public async Task CreateOrderAsync(Order order)
        {
            if (order.Total <= 0)
            {
                throw new InvalidOperationException("O total do pedido deve ser maior que zero.");
            }
            
            try
            {
                _context.Entry(order).State = EntityState.Added;
                _context.OrderProducts.AddRange(order.OrderProducts);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Erro ao salvar o pedido no banco de dados.", ex);
            }
        }

        // Método implementado para editar o status do pedido
        public async Task UpdateOrderStatusAsync(Guid orderId, Status newStatus)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new InvalidOperationException("Pedido não encontrado.");
            }
            try
            {
                order.Status = newStatus;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Erro ao atualizar o status do pedido.", ex);
            }
        }
    }
}