using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class ModelosService(
IDbContextFactory<Contexto> contextFactory
) : Aplicada1.Core.IService<Modelo1, int>
{
    public Task<Modelo1?> Buscar(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Eliminar(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Modelo1>> GetList(Expression<Func<Modelo1, bool>> criterio)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Guardar(Modelo1 entidad)
    {
        throw new NotImplementedException();
    }
}

