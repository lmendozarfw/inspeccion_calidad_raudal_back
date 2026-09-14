using Microsoft.EntityFrameworkCore;
using Calidad_API.Models;

namespace Calidad_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Seguridad
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<UsuarioRol> UsuarioRoles { get; set; } = null!;
        public DbSet<PermisoArea> PermisosArea { get; set; } = null!;

        // Catálogos
        public DbSet<Departamento> Departamentos { get; set; } = null!;
        public DbSet<CentroTrabajo> CentrosTrabajo { get; set; } = null!;
        public DbSet<Criticidad> Criticidades { get; set; } = null!;
        public DbSet<TipoInspeccion> TiposInspeccion { get; set; } = null!;
        public DbSet<Pieza> Piezas { get; set; } = null!;
        public DbSet<Modelo> Modelos { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<Defecto> Defectos { get; set; } = null!;
        public DbSet<DefectoTipoInspeccion> DefectoTiposInspeccion { get; set; } = null!;

        // Operación
        public DbSet<Transfer> Transfers { get; set; } = null!;
        public DbSet<Inspeccion> Inspecciones { get; set; } = null!;
        public DbSet<InspeccionDetalle> InspeccionDetalles { get; set; } = null!;
        public DbSet<Vale> Vales { get; set; } = null!;

        // Métricas
        public DbSet<MetaCalidad> MetasCalidad { get; set; } = null!;
        public DbSet<ProduccionDiaria> ProduccionesDiarias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // —— Seguridad ——
            modelBuilder.Entity<UsuarioRol>(e =>
            {
                e.HasKey(x => new { x.IdUsuario, x.IdRol });
                e.HasOne(x => x.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PermisoArea>(e =>
            {
                e.HasKey(x => new { x.IdUsuario, x.IdArea });
                e.HasOne(x => x.Usuario).WithMany(u => u.PermisosArea).HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Area).WithMany(a => a.PermisosArea).HasForeignKey(x => x.IdArea).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Usuario>().HasIndex(x => x.Username).IsUnique();
            modelBuilder.Entity<Rol>().HasIndex(x => x.Codigo).IsUnique();

            // —— Catálogos ——
            modelBuilder.Entity<Departamento>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<CentroTrabajo>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<Criticidad>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<TipoInspeccion>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<Pieza>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<Modelo>().HasIndex(x => x.Codigo).IsUnique();
            modelBuilder.Entity<Area>().HasIndex(x => x.Codigo).IsUnique();

            modelBuilder.Entity<Defecto>(e =>
            {
                e.HasIndex(x => new { x.IdArea, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.IdArea, x.Nombre }).IsUnique();
                e.HasOne(x => x.Area).WithMany(a => a.Defectos).HasForeignKey(x => x.IdArea).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Criticidad).WithMany(c => c.Defectos).HasForeignKey(x => x.IdCriticidad).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Pieza).WithMany(p => p.Defectos).HasForeignKey(x => x.IdPieza).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DefectoTipoInspeccion>(e =>
            {
                e.HasKey(x => new { x.IdDefecto, x.IdTipoInspeccion });
                e.HasOne(x => x.Defecto).WithMany(d => d.DefectoTiposInspeccion).HasForeignKey(x => x.IdDefecto).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.TipoInspeccion).WithMany(t => t.DefectoTiposInspeccion).HasForeignKey(x => x.IdTipoInspeccion).OnDelete(DeleteBehavior.Restrict);
            });

            // —— Operación ——
            modelBuilder.Entity<Transfer>(e =>
            {
                e.HasIndex(x => x.Lote);
                e.HasIndex(x => x.QrRaw);
                e.HasOne(x => x.Modelo).WithMany(m => m.Transfers).HasForeignKey(x => x.IdModelo).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Inspeccion>(e =>
            {
                e.HasIndex(x => x.FechaInspeccion);
                e.HasIndex(x => new { x.IdArea, x.FechaInspeccion });
                e.HasOne(x => x.Transfer).WithMany(t => t.Inspecciones).HasForeignKey(x => x.IdTransfer).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Area).WithMany(a => a.Inspecciones).HasForeignKey(x => x.IdArea).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.TipoInspeccion).WithMany(t => t.Inspecciones).HasForeignKey(x => x.IdTipoInspeccion).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Usuario).WithMany(u => u.Inspecciones).HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InspeccionDetalle>(e =>
            {
                e.HasIndex(x => x.IdDefecto);
                e.HasIndex(x => x.FechaRegistro);
                e.HasOne(x => x.Inspeccion).WithMany(i => i.Detalles).HasForeignKey(x => x.IdInspeccion).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Defecto).WithMany(d => d.InspeccionDetalles).HasForeignKey(x => x.IdDefecto).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Usuario).WithMany(u => u.InspeccionDetalles).HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Vale>(e =>
            {
                e.HasIndex(x => x.Folio).IsUnique();
                e.HasOne(x => x.InspeccionDetalle).WithMany(d => d.Vales).HasForeignKey(x => x.IdInspeccionDetalle).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Pieza).WithMany(p => p.Vales).HasForeignKey(x => x.IdPieza).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.UsuarioSolicita).WithMany(u => u.ValesSolicitados).HasForeignKey(x => x.IdUsuarioSolicita).OnDelete(DeleteBehavior.Restrict);
            });

            // —— Métricas ——
            modelBuilder.Entity<MetaCalidad>(e =>
            {
                e.HasIndex(x => new { x.IdArea, x.Anio, x.Semana }).IsUnique();
                e.HasOne(x => x.Area).WithMany(a => a.MetasCalidad).HasForeignKey(x => x.IdArea).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProduccionDiaria>(e =>
            {
                e.HasIndex(x => new { x.IdArea, x.Fecha, x.Turno }).IsUnique();
                e.HasOne(x => x.Area).WithMany(a => a.ProduccionesDiarias).HasForeignKey(x => x.IdArea).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}