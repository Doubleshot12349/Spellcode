using System;

[Serializable]
public struct HexCoords : IEquatable<HexCoords>
{
    public int q; // column like
    public int r; // row like

    public HexCoords(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public bool Equals(HexCoords other) => q == other.q && r == other.r;
    public override bool Equals(object obj) => obj is HexCoords other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (q * 397) ^ r;
        }
    }

    public static bool operator ==(HexCoords a, HexCoords b) => a.Equals(b);
    public static bool operator !=(HexCoords a, HexCoords b) => !a.Equals(b);

    public override string ToString() => $"({q},{r})";
}
