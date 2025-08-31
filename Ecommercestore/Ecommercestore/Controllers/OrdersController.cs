using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceStore.Data;
using EcommerceStore.Models;
using EcommerceStore.Models.DTOs;

namespace EcommerceStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderResponses = orders.Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.UserName,
                    UserEmail = o.UserEmail,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        Total = oi.Total
                    }).ToList()
                }).ToList();

                return Ok(orderResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                var orderResponse = new OrderResponseDto
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserName = order.UserName,
                    UserEmail = order.UserEmail,
                    ShippingAddress = order.ShippingAddress,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        Total = oi.Total
                    }).ToList()
                };

                return Ok(orderResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving order", error = ex.Message });
            }
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create the order
                var order = new Order
                {
                    UserId = createOrderDto.UserId,
                    UserName = createOrderDto.UserName,
                    UserEmail = createOrderDto.UserEmail,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    TotalAmount = createOrderDto.TotalAmount,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var itemDto in createOrderDto.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        ProductName = itemDto.ProductName,
                        Price = itemDto.Price,
                        Quantity = itemDto.Quantity,
                        Total = itemDto.Total
                    };

                    _context.OrderItems.Add(orderItem);

                    // Update product stock
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product != null)
                    {
                        if (product.Stock < itemDto.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = $"Insufficient stock for product {product.Name}" });
                        }
                        product.Stock -= itemDto.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the created order
                var orderResponse = new OrderResponseDto
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserName = order.UserName,
                    UserEmail = order.UserEmail,
                    ShippingAddress = order.ShippingAddress,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Items = createOrderDto.Items
                };

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error creating order", error = ex.Message });
            }
        }

        // PUT: api/orders/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(new { message = "Invalid status. Valid statuses are: " + string.Join(", ", validStatuses) });
                }

                order.Status = status;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating order status", error = ex.Message });
            }
        }

        // DELETE: api/orders/5 (Cancel order)
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                if (order.Status == "Delivered")
                {
                    return BadRequest(new { message = "Cannot cancel a delivered order" });
                }

                // Restore product stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }

                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error cancelling order", error = ex.Message });
            }
        }
    }
}