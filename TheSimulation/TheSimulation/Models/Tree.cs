using System.Drawing;
using System.Windows.Media;

namespace TheSimulation;

public sealed record Tree
(
    Point Position,
    double Size,
    Brush Color
);
