using Microsoft.AspNetCore.Mvc;
using PurchaseOrderAPI.Application.Services;
using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.API.Controllers
{
    // Controller que gerencia todos os endpoints relacionados a pedidos de compra
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        // Referências aos services
        private readonly PurchaseOrderService _purchaseOrderService;
        private readonly ApprovalService _approvalService;
        private readonly AuditService _auditService;

        // Construtor que recebe os services
        public PurchaseOrdersController(
            PurchaseOrderService purchaseOrderService,
            ApprovalService approvalService,
            AuditService auditService)
        {
            _purchaseOrderService = purchaseOrderService;
            _approvalService = approvalService;
            _auditService = auditService;
        }

        // Create (POST) cria um novo pedido de compra
        // POST /api/purchaseorders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            try
            {
                // Valida se os dados foram fornecidos
                if (string.IsNullOrWhiteSpace(dto.OrderNumber))
                    return BadRequest("Número do pedido é obrigatório");

                if (dto.ElaboratorId == Guid.Empty)
                    return BadRequest("ID do elaborador é obrigatório");

                // Chama o service para criar o pedido
                var order = await _purchaseOrderService.CreatePurchaseOrderAsync(
                    dto.ElaboratorId,
                    dto.OrderNumber,
                    dto.Description ?? ""
                );

                // Retorna 201 Created com a URL e dados do novo pedido
                return Created($"/api/purchaseorders/{order.Id}", new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    status = order.Status,
                    createdAt = order.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Read (GET) retorna todos os pedidos
        // GET /api/purchaseorders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _purchaseOrderService.GetAllPurchaseOrdersAsync();

                return Ok(new
                {
                    success = true,
                    data = orders.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        totalValue = o.TotalValue,
                        status = o.Status,
                        approvalTier = o.ApprovalTier,
                        createdAt = o.CreatedAt,
                        itemCount = o.Items.Count
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna um pedido específico pelo ID
        // GET /api/purchaseorders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var order = await _purchaseOrderService.GetPurchaseOrderAsync(id);

                if (order == null)
                    return NotFound(new { success = false, message = "Pedido não encontrado" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = order.Id,
                        orderNumber = order.OrderNumber,
                        elaboratorId = order.ElaboratorId,
                        totalValue = order.TotalValue,
                        status = order.Status,
                        approvalTier = order.ApprovalTier,
                        description = order.Description,
                        createdAt = order.CreatedAt,
                        items = order.Items.Select(i => new
                        {
                            id = i.Id,
                            description = i.Description,
                            quantity = i.Quantity,
                            unitPrice = i.UnitPrice,
                            subtotal = i.Quantity * i.UnitPrice
                        }),
                        approvals = order.Approvals.Select(a => new
                        {
                            id = a.Id,
                            approverRole = a.ApproverRole,
                            status = a.Status,
                            sequence = a.Sequence,
                            actionDate = a.ActionDate
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna pedidos por status
        // GET /api/purchaseorders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            try
            {
                // Converte string para enum
                if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                    return BadRequest("Status inválido");

                var orders = await _purchaseOrderService.GetPurchaseOrdersByStatusAsync(orderStatus);

                return Ok(new
                {
                    success = true,
                    count = orders.Count,
                    data = orders.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        totalValue = o.TotalValue,
                        status = o.Status,
                        createdAt = o.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna pedidos de um usuário
        // GET /api/purchaseorders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(Guid userId)
        {
            try
            {
                var orders = await _purchaseOrderService.GetUserPurchaseOrdersAsync(userId);

                return Ok(new
                {
                    success = true,
                    count = orders.Count,
                    data = orders.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        totalValue = o.TotalValue,
                        status = o.Status,
                        createdAt = o.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna pedidos aguardando aprovação
        // GET /api/purchaseorders/pending/approvals
        [HttpGet("pending/approvals")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var orders = await _purchaseOrderService.GetPendingApprovalsAsync();

                return Ok(new
                {
                    success = true,
                    count = orders.Count,
                    data = orders.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        totalValue = o.TotalValue,
                        approvalTier = o.ApprovalTier,
                        createdAt = o.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Update (PUT) edita um pedido apenas se estiver em Draft
        // PUT /api/purchaseorders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditOrder(Guid id, [FromBody] EditPurchaseOrderDto dto)
        {
            try
            {
                await _purchaseOrderService.EditPurchaseOrderAsync(id, dto.Description ?? "");

                return Ok(new
                {
                    success = true,
                    message = "Pedido atualizado com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        // Adiciona um item ao pedido
        // POST /api/purchaseorders/{id}/items
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItemToOrder(Guid id, [FromBody] AddItemDto dto)
        {
            try
            {
                await _purchaseOrderService.AddItemToPurchaseOrderAsync(
                    id,
                    dto.Description,
                    dto.Quantity,
                    dto.UnitPrice
                );

                return Ok(new
                {
                    success = true,
                    message = "Item adicionado com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        // Envia um pedido para aprovação
        // POST /api/purchaseorders/{id}/submit
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitForApproval(Guid id, [FromBody] SubmitForApprovalDto dto)
        {
            try
            {
                await _purchaseOrderService.SubmitForApprovalAsync(id, dto.ElaboratorId);

                return Ok(new
                {
                    success = true,
                    message = "Pedido enviado para aprovação"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Aprova um pedido
        // POST /api/purchaseorders/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApprovePurchaseOrder(Guid id, [FromBody] ApproveOrderDto dto)
        {
            try
            {
                await _approvalService.ApprovePurchaseOrderAsync(id, dto.ApproverId, dto.Comments ?? "");

                return Ok(new
                {
                    success = true,
                    message = "Pedido aprovado com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Rejeita um pedido
        // POST /api/purchaseorders/{id}/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectPurchaseOrder(Guid id, [FromBody] RejectOrderDto dto)
        {
            try
            {
                await _approvalService.RejectPurchaseOrderAsync(id, dto.ApproverId, dto.Reason);

                return Ok(new
                {
                    success = true,
                    message = "Pedido rejeitado com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Solicita revisão de um pedido
        // POST /api/purchaseorders/{id}/request-revision
        [HttpPost("{id}/request-revision")]
        public async Task<IActionResult> RequestRevision(Guid id, [FromBody] RequestRevisionDto dto)
        {
            try
            {
                await _approvalService.RequestRevisionAsync(id, dto.ApproverId, dto.Reason);

                return Ok(new
                {
                    success = true,
                    message = "Revisão solicitada com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna a cadeia de aprovações de um pedido
        // GET /api/purchaseorders/{id}/approvals
        [HttpGet("{id}/approvals")]
        public async Task<IActionResult> GetApprovalChain(Guid id)
        {
            try
            {
                var approvals = await _approvalService.GetApprovalChainAsync(id);

                return Ok(new
                {
                    success = true,
                    count = approvals.Count,
                    data = approvals.Select(a => new
                    {
                        id = a.Id,
                        sequence = a.Sequence,
                        approverRole = a.ApproverRole,
                        status = a.Status,
                        actionDate = a.ActionDate,
                        comments = a.Comments
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Cancela um pedido
        // DELETE /api/purchaseorders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto dto)
        {
            try
            {
                await _purchaseOrderService.CancelPurchaseOrderAsync(id, dto.UserId, dto.Reason);

                return Ok(new
                {
                    success = true,
                    message = "Pedido cancelado com sucesso"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna o histórico de um pedido
        // GET /api/purchaseorders/{id}/history
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetOrderHistory(Guid id)
        {
            try
            {
                var history = await _auditService.GetOrderHistoryAsync(id);

                return Ok(new
                {
                    success = true,
                    count = history.Count,
                    data = history.Select(log => new
                    {
                        id = log.Id,
                        action = log.Action,
                        timestamp = log.Timestamp,
                        details = log.Details
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }


    // DTO para criar pedido
    public class CreatePurchaseOrderDto
    {
        public Guid ElaboratorId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // DTO para editar pedido
    public class EditPurchaseOrderDto
    {
        public string? Description { get; set; }
    }

    // DTO para adicionar item
    public class AddItemDto
    {
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // DTO para submeter para aprovação
    public class SubmitForApprovalDto
    {
        public Guid ElaboratorId { get; set; }
    }

    // DTO para aprovar pedido
    public class ApproveOrderDto
    {
        public Guid ApproverId { get; set; }
        public string? Comments { get; set; }
    }

    // DTO para rejeitar pedido
    public class RejectOrderDto
    {
        public Guid ApproverId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // DTO para solicitar revisão
    public class RequestRevisionDto
    {
        public Guid ApproverId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // DTO para cancelar pedido
    public class CancelOrderDto
    {
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}