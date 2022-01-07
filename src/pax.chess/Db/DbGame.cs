using System.ComponentModel.DataAnnotations;

namespace pax.chess;
public class DbGame
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string? Event { get; set; }
    [MaxLength(100)]
    public string? Site { get; set; }
    [Required]
    public DateOnly UTCDate { get; set; }
    [Required]
    public TimeOnly UTCTime { get; set; }
    [MaxLength(100)]
    public string? White { get; set; }
    [MaxLength(100)]
    public string? Black { get; set; }
    public short WhiteElo { get; set; }
    public short BlackElo { get; set; }
    public Variant Variant { get; set; }
    [MaxLength(3)]
    public string? ECO { get; set; }
    [MaxLength(100)]
    public string? Opening { get; set; }
    [MaxLength(100)]
    public string? Annotator { get; set; }
    [MaxLength(100)]
    public string? TimeControl { get; set; }
    public Result Result { get; set; }
    public Termination Termination { get; set; }
    public string EngineMoves { get; set; } = String.Empty;
    public int HalfMoves { get; set; }
    public virtual ICollection<DbPosition> Positions { get; set; }
    public virtual ICollection<DbVariation> Variations { get; set; }
    public DbGame()
    {
        Positions = new HashSet<DbPosition>();
        Variations = new HashSet<DbVariation>();
    }
}

public class DbPosition
{
    public int Id { get; set; }
    public uint Position { get; set; }
    public virtual ICollection<DbGame> Games { get; set; }

    public DbPosition()
    {
        Games = new HashSet<DbGame>();
    }
}

public class DbVariation
{
    public int Id { get; set; }
    public int StartMove { get; set; }
    public string EngineMoves { get; set; } = String.Empty;
    public DbEvaluation? Evaluation { get; set; }
    public virtual ICollection<DbSubVariation> SubVariations { get; set; }

    public DbVariation()
    {
        SubVariations = new HashSet<DbSubVariation>();
    }
}

public class DbSubVariation
{
    public int Id { get; set; }
    public int RootStartMove { get; set; }
    public string EngineMovesWithSubs { get; set; } = String.Empty;
    public DbVariation RootVariation { get; set; }
}

public class DbEvaluation
{
    public int Id { get; set; }
    public short Score { get; set; }
    public sbyte Mate { get; set; }
    public bool IsBlack { get; set; }
}

