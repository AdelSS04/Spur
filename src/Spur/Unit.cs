namespace Spur;

/// <summary>
/// Represents the absence of a meaningful return value.
/// Used with <c>Result&lt;Unit&gt;</c> when an operation succeeds but produces no value —
/// the functional equivalent of <c>void</c> as a generic type argument.
/// </summary>
/// <remarks>
/// Use <c>Result&lt;Unit&gt;</c> instead of <c>Result&lt;bool&gt;</c> for void operations.
/// The ASP.NET Core extension <c>ToHttpResult()</c> maps <c>Result&lt;Unit&gt;</c> to HTTP 204 No Content.
/// </remarks>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>The singleton Unit value.</summary>
    public static readonly Unit Value = default;

    /// <summary>Determines whether the specified Unit is equal to the current Unit.</summary>
    public bool Equals(Unit other) => true;

    /// <summary>Compares the current Unit with another Unit.</summary>
    public int CompareTo(Unit other) => 0;

    /// <summary>Determines whether the specified object is equal to the current Unit.</summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>Returns the hash code for this Unit.</summary>
    public override int GetHashCode() => 0;

    /// <summary>Returns a string representation of this Unit.</summary>
    public override string ToString() => "()";

    /// <summary>Determines whether two Unit values are equal.</summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>Determines whether two Unit values are not equal.</summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
