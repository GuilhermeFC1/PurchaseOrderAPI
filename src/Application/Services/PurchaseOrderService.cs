using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Repositories;

namespace PurchaseOrderAPI.Application.Services
{
    // Implementa a lógica de negócio para pedidos de compra
    public class PurchaseOrderService
    {
        // Referências aos repositorios
        private readonly PurchaseOrderRepository _orderRepository;
        private readonly PurchaseOrderItemRepository _itemRepository;
        private readonly PurchaseOrderApprovalRepository _approvalRepository;
        private readonly AuditLogRepository _auditRepository;
        private readonly UserRepository _userRepository;

        // Construtor que recebe os repositorios
        public PurchaseOrderService(
            PurchaseOrderRepository orderRepository,
            PurchaseOrderItemRepository itemRepository,
            PurchaseOrderApprovalRepository approvalRepository,
            AuditLogRepository auditRepository,
            UserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _approvalRepository = approvalRepository;
            _auditRepository = auditRepository;
            _userRepository = userRepository;
        }

        // Create, cria um novo pedido de compra
        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(
            Guid elaboratorId,
            string orderNumber,
            string description = "")
        {
            // Valida se o elaborador existe
            var elaborator = await _userRepository.GetByIdAsync(elaboratorId);
            if (elaborator == null)
                throw new InvalidOperationException("Elaborador não encontrado");

            // Valida se o número do pedido já existe
            var existingOrder = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (existingOrder != null)
                throw new InvalidOperationException("Número do pedido já existe");

            // Valida se o número do pedido não está vazio
            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentException("Número do pedido é obrigatório");

            // Cria o novo pedido
            var order = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                ElaboratorId = elaboratorId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                Description = description,
                TotalValue = 0,
                ApprovalTier = ApprovalTier.Tier1 
            };

            // Salva no banco
            await _orderRepository.AddAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                order.Id,
                elaboratorId,
                "Pedido criado",
                $"Número: {orderNumber}"
            );
            await _auditRepository.AddAsync(auditLog);

            return order;
        }

        // Read, busca um pedido pelo ID
        public async Task<PurchaseOrder?> GetPurchaseOrderAsync(Guid id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        // Retorna todos os pedidos
        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        // Retorna pedidos de um usuário
        public async Task<List<PurchaseOrder>> GetUserPurchaseOrdersAsync(Guid userId)
        {
            return await _orderRepository.GetByElaboratorAsync(userId);
        }

        // Retorna pedidos por status
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetByStatusAsync(status);
        }

        // Retorna pedidos aguardando aprovação
        public async Task<List<PurchaseOrder>> GetPendingApprovalsAsync()
        {
            return await _orderRepository.GetPendingApprovalsAsync();
        }

        // Update, edita um pedido apenas se estiver em Draft
        public async Task EditPurchaseOrderAsync(
            Guid orderId,
            string description)
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se está em Draft
            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem ser editados");

            // Atualiza
            order.Description = description;
            order.UpdatedAt = DateTime.UtcNow;

            // Salva
            await _orderRepository.UpdateAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                order.ElaboratorId,
                "Pedido editado",
                description
            );
            await _auditRepository.AddAsync(auditLog);
        }

        // Adiciona um item ao pedido
        public async Task AddItemToPurchaseOrderAsync(
            Guid orderId,
            string description,
            int quantity,
            decimal unitPrice)
        {
            // Valida quantidade
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            // Valida preço
            if (unitPrice < 0)
                throw new ArgumentException("Preço não pode ser negativo");

            // Valida descrição
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descrição é obrigatória");

            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se está em Draft
            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem receber itens");

            // Cria o item
            var item = new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                CreatedAt = DateTime.UtcNow
            };

            // Salva o item
            await _itemRepository.AddAsync(item);

            // Atualiza valor total e alçada do pedido
            order.TotalValue = await _itemRepository.GetTotalValueByOrderAsync(orderId);
            order.ApprovalTier = order.DetermineApprovalTier();
            await _orderRepository.UpdateAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                order.ElaboratorId,
                "Item adicionado",
                $"Descrição: {description}, Qtd: {quantity}, Preço: {unitPrice:C}"
            );
            await _auditRepository.AddAsync(auditLog);
        }

        // Submete um pedido para aprovação, cria a cadeia de aprovações
        public async Task SubmitForApprovalAsync(Guid orderId, Guid elaboratorId)
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se é o criador do pedido
            if (order.ElaboratorId != elaboratorId)
                throw new InvalidOperationException("Apenas o criador pode submeter para aprovação");

            // Valida se está em Draft
            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem ser submetidos");

            // Valida se tem itens
            var itemCount = await _itemRepository.GetItemCountByOrderAsync(orderId);
            if (itemCount == 0)
                throw new InvalidOperationException("Pedido deve ter pelo menos um item");

            // Determina a alçada
            var tier = order.DetermineApprovalTier();
            order.ApprovalTier = tier;

            // Cria a cadeia de aprovações baseada na alçada
            var approvals = CreateApprovalChain(orderId, tier);
            await _approvalRepository.AddRangeAsync(approvals);

            // Atualiza status do pedido
            order.Status = OrderStatus.InApproval;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                elaboratorId,
                "Pedido submetido para aprovação",
                $"Alçada: {tier}, Total: {order.TotalValue:C}"
            );
            await _auditRepository.AddAsync(auditLog);
        }


        // Cancela um pedido
        public async Task CancelPurchaseOrderAsync(Guid orderId, Guid userId, string reason)
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se já está cancelado
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Pedido já foi cancelado");

            // Valida se tem motivo
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Motivo do cancelamento é obrigatório");

            // Cancela o pedido
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                userId,
                "Pedido cancelado",
                reason
            );
            await _auditRepository.AddAsync(auditLog);
        }


        // Cria a cadeia de aprovações baseada na alçada
        private List<PurchaseOrderApproval> CreateApprovalChain(Guid orderId, ApprovalTier tier)
        {
            var approvals = new List<PurchaseOrderApproval>();

            // Sempre começa com suprimentos, Tier1
            approvals.Add(new PurchaseOrderApproval
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ApproverId = null, 
                ApproverRole = UserRole.Supplies,
                Status = ApprovalStatus.Pending,
                Sequence = 1
            });

            // Se for Tier2 ou Tier3, adiciona Manager
            if (tier == ApprovalTier.Tier2 || tier == ApprovalTier.Tier3)
            {
                approvals.Add(new PurchaseOrderApproval
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ApproverId = null,
                    ApproverRole = UserRole.Manager,
                    Status = ApprovalStatus.Pending,
                    Sequence = 2
                });
            }

            // Se for Tier3, adiciona Director
            if (tier == ApprovalTier.Tier3)
            {
                approvals.Add(new PurchaseOrderApproval
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ApproverId = null,
                    ApproverRole = UserRole.Director,
                    Status = ApprovalStatus.Pending,
                    Sequence = 3
                });
            }

            return approvals;
        }

        // Verifica se um pedido está completo, todas as aprovações
        public async Task<bool> IsPurchaseOrderCompleteAsync(Guid orderId)
        {
            return await _approvalRepository.AllApprovalsCompleteAsync(orderId);
        }
    }
}