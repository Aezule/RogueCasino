using System.Collections.Generic;

public class HexCase
{
    public int q;           // colonne (depth, gauche→droite)
    public int r;           // lane   (0=bas, width-1=haut)
    public int depth;       // profondeur dans la map
    public CaseType type;
    public bool isVisited = false;

    public List<HexCase> neighbors = new List<HexCase>();

    public HexCase(int q, int r, CaseType type, int depth)
    {
        this.q = q;
        this.r = r;
        this.type = type;
        this.depth = depth;
    }
}
