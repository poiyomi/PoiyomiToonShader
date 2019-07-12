[maxvertexcount(3)]
void geom(triangle v2f IN[3], inout TriangleStream < v2f > tristream)
{
    for (int i = 0; i < 3; i ++)
    {
        IN[i].uv = uv;
        tristream.Append(IN[i]);
    }
    tristream.RestartStrip();
}