    public class InventoryImport
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public long? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "COMPLETED";

        public string? Note { get; set; }

        public long? CreatedBy { get; set; }
        public long? ApprovedBy { get; set; }

        public User? CreatedByUser { get; set; }
        public User? ApprovedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }

        public bool TaxDeclared { get; set; } = false;
        public long? TaxDeclarationId { get; set; }

        public TaxDeclaration? TaxDeclaration { get; set; }

        public ICollection<InventoryImportItem> Items { get; set; } = new List<InventoryImportItem>();
        public virtual ICollection<TaxDeclarationDetail> TaxDeclarationDetails { get; set; }
        = new List<TaxDeclarationDetail>();
    }