﻿/*
 * https://en.wikipedia.org/wiki/Visitor_pattern
 */

public interface IPlantVisitor
{
    void VisitPlant(Plant plant); 
}

public interface ICameraVisitor
{
    void VisitCamera(CameraController camera);
}