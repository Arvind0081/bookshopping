using BookShoppingProject.DataAccess.Data;
using BookShoppingProject.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShoppingProject.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            category = new CategoryRepository(_context);
            SP_CALLS = new SP_CALLS(_context);
            coverType = new CoverTypeRepository(_context);
            Product = new ProductRepository(_context);
            ApplicationUser = new ApplicationUserRepository(_context);
            Company = new CompanyRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            OrderHeader = new OrderHeaderRepository(_context);
            OrderDetail = new OrderDetailRepository(_context);
        }
        public ICategoryRepository category { get; private set; }

        public ISP_CALLS SP_CALLS { get; private set; }

        public ICoverTypeRepository coverType { get; private set; }

        public IProductRepository Product { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public ICompanyRepository Company { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
