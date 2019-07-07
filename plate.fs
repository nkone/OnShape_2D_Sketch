# **************************************************************************** #
#                                                                              #
#                                                         :::      ::::::::    #
#    plate.fs                                           :+:      :+:    :+:    #
#                                                     +:+ +:+         +:+      #
#    By: phtruong <marvin@42.fr>                    +#+  +:+       +#+         #
#                                                 +#+#+#+#+#+   +#+            #
#    Created: 2019/07/07 16:38:10 by phtruong          #+#    #+#              #
#    Updated: 2019/07/07 16:44:17 by phtruong         ###   ########.fr        #
#                                                                              #
# **************************************************************************** #

FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "Plate" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // Define the parameters of the feature type
        annotation { "Name" : "Enter width (mm)" }
        isLength(definition.width, { (millimeter) : [1, 100, 3000]} as LengthBoundSpec);
        annotation { "Name" : "Slot Diameter(mm)" }
        isLength(definition.slotDia, { (millimeter) : [1, 10, 2000] } as LengthBoundSpec);
        annotation { "Name" : "Slot offset from side" }
        isLength(definition.slotFromSide, { (millimeter) : [1, 10, 2000] } as LengthBoundSpec);
        annotation { "Name" : "Mirror offset" }
        isLength(definition.mirrorOffset, { (millimeter) : [1, 5, 2000] } as LengthBoundSpec);
        annotation { "Name" : "Slot1 from top" }
        isLength(definition.slot1, { (millimeter) : [1, 15, 2000] } as LengthBoundSpec);
        annotation { "Name" : "Slot2 from top" }
        isLength(definition.slot2, { (millimeter) : [1, 20, 2000] } as LengthBoundSpec);
        annotation { "Name" : "Slot3 from top" }
        isLength(definition.slot3, { (millimeter) : [1, 20, 2000] } as LengthBoundSpec);
    }
    {
        // Extract numbers from input dimensions
        var width is number = definition.width / millimeter;
        var offset is number = definition.mirrorOffset / millimeter;
        var sideOffset is number = definition.slotFromSide / millimeter;
        var slotDia is number = definition.slotDia / millimeter;
        var slotOneOffset is number = definition.slot1 / millimeter;
        var slotTwoOffset is number = definition.slot2 / millimeter;
        var slotThreeOffset is number = definition.slot3 / millimeter;
        // Set global variables for slots offset
        setVariable(context, "slotOneOff", slotOneOffset);
        setVariable(context, "slotTwoOff", slotTwoOffset);
        setVariable(context, "slotThreeOff", slotThreeOffset);
        //Sketch the main image
        sketchBase(context, id, width, offset, sideOffset, slotDia);
    });
    /*
    ** function constructionLength:
    ** Find the construction length for to construct the slot
    ** Parameters:
    ** [width] is length of the plate
    ** [offsetX] is the x offset of the mirror line from the origin (0,0)
    ** [slotOffset] is the slot distance below the (0, width)
    ** [slotDia] is the slot diameter for all slots
    ** [sideOffset] is the slot offset from the side
    ** Return: a number (constructionLength)
    */
    function constructionLength(width is number, offsetX is number, slotOffset is number, slotDia is number, sideOffset is number)
    {
        var constructLength is number = width - (slotOffset + (offsetX) + ((slotDia/2)/sin(45 * degree)) + sideOffset);
        return constructLength;
    }
    /*
    ** function sketchBase:
    ** Recreate the 2D sketch in FeatureScript for faster changing dimension of the sketch via user inputs.
    ** Parameters:
    ** [context] is Context (built in data structure)
    ** [id] is Id (built in data structure)
    ** [width] is length of the plate
    ** [offset] is mirror offset from the center diagonal of the plate
    ** [sideOffset] is the slot offset from the side
    ** [slotDia] is slot diameter for all slots
    ** Default unit: millimeter
    ** Functionality: Changes in FeatureScript allows faster visualization than changing variables in normal sketch
	** Return: NULL
    */
    function sketchBase(context is Context, id is Id, width is number, offset is number, sideOffset is number, slotDia is number)
    {
        var baseSketch is Sketch = newSketch(context, id + "base", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        // By dividing the offset (distance between the 45 degrees center and the tangent offset) to sin(45), we can get the x distance from the vertex.
        var offsetX is number = offset/sin(45 * degree);
        var slotOneOffset = getVariable(context, "slotOneOff");
        var slotTwoOffset = slotOneOffset+getVariable(context, "slotTwoOff");
        var slotThreeOffset = slotTwoOffset+getVariable(context, "slotThreeOff");
        // Sketch the base and contructions for the slots
        skRectangle(baseSketch, "base", {
                "firstCorner" : vector(0, 0) * millimeter,
                "secondCorner" : vector(width, width) * millimeter
        });
        skLineSegment(baseSketch, "mirror", {
                "start" : vector(0, width) * millimeter,
                "end" : vector(width, 0) * millimeter,
                "construction": true
        });
        skLineSegment(baseSketch, "offset", {
                "start" : vector(offsetX, width) * millimeter,
                "end" : vector(width, offsetX) * millimeter,
                "construction" : true
        });
        sketchSlot(context, id, baseSketch, width, offsetX, slotOneOffset, slotDia, sideOffset, 1);
        sketchSlot(context, id, baseSketch, width, offsetX, slotTwoOffset, slotDia, sideOffset, 2);
        sketchSlot(context, id, baseSketch, width, offsetX, slotThreeOffset, slotDia, sideOffset, 3);
        skSolve(baseSketch);
    }
    /*
    ** function sketchSlot:
    ** Sketches the slots tangent to the mirror line
    ** Parameters:
    ** [context] is Context (built in data structure)
    ** [id] is Id (built in data structure)
    ** [slot] is Sketch (variable for a sketch in FS)
    ** [offsetX] is the x offset of the mirror line from the origin (0,0)
    ** [slotOffset] is the slot distance below the (0, width)
    ** [slotDia] is the slot diameter
    ** [sideOffset] is the slot offset from the side of the plate
    ** [idx] is index for slot number
    ** Default unit: millimeter
    ** Functionality: Can be resuse to draw mirror slots across a 45 degree diagonal offset
	** Return: NULL
    */
    function sketchSlot(context is Context, id is Id, slot is Sketch, width is number, offsetX is number, slotOffset is number, slotDia is number, sideOffset is number, idx is number)
    {
        var constructLength is number= constructionLength(width, offsetX, slotOffset, slotDia, sideOffset);
        skLineSegment(slot, "slotConstruction" ~ idx, {
                "start" : vector(width-sideOffset, width-slotOffset) * millimeter,
                "end" : vector(width-sideOffset-constructLength, width-slotOffset) * millimeter,
                "construction" : true
        });
        skLineSegment(slot, "lineBottom" ~ idx, {
                "start" : vector(width-sideOffset, width-(slotOffset-(slotDia/2))) * millimeter,
                "end" : vector(width-sideOffset-constructLength, width-(slotOffset-(slotDia/2))) * millimeter
        });
        skLineSegment(slot, "lineTop" ~ idx, {
                "start" : vector(width-sideOffset, width-(slotOffset+(slotDia/2))) * millimeter,
                "end" : vector(width-sideOffset-constructLength, width-(slotOffset+(slotDia/2))) * millimeter
        });
        skArc(slot, "slotArcLeft" ~ idx, {
                "start" : vector(width-sideOffset-constructLength, width-(slotOffset-(slotDia/2))) * millimeter,
                "mid" : vector(width-sideOffset-(constructLength+(slotDia/2)), width-slotOffset) * millimeter,
                "end" : vector(width-sideOffset-constructLength, width-(slotOffset+(slotDia/2))) * millimeter
        });
        skArc(slot, "slotArcRight" ~ idx, {
                "start" : vector(width-sideOffset, width-(slotOffset-(slotDia/2))) * millimeter,
                "mid" : vector(width-sideOffset+(slotDia/2), width-slotOffset) * millimeter,
                "end" : vector(width-sideOffset, width-(slotOffset+(slotDia/2))) * millimeter
        });
        skLineSegment(slot, "lineMirrorLeft" ~ idx, {
                "start" : vector(slotOffset-(slotDia/2), sideOffset) * millimeter,
                "end" : vector(slotOffset-(slotDia/2), sideOffset+constructLength) * millimeter
        });
        skLineSegment(slot, "lineMirrorRight" ~ idx, {
                "start" : vector(slotOffset-(slotDia/2)+slotDia, sideOffset) * millimeter,
                "end" : vector(slotOffset-(slotDia/2)+slotDia, sideOffset+constructLength) * millimeter
        });
        skArc(slot, "slotArcMirrorBot" ~ idx, {
                "start" : vector(slotOffset-(slotDia/2), sideOffset) * millimeter,
                "mid" : vector(slotOffset-(slotDia/2)+(slotDia/2), sideOffset-(slotDia/2)) * millimeter,
                "end" : vector(slotOffset-(slotDia/2)+slotDia, sideOffset) * millimeter
        });
        skArc(slot, "slotArcMirrorTop" ~ idx, {
                "start" : vector(slotOffset-(slotDia/2), sideOffset+constructLength) * millimeter,
                "mid" : vector(slotOffset-(slotDia/2)+(slotDia/2), sideOffset+constructLength+(slotDia/2)) * millimeter,
                "end" : vector(slotOffset-(slotDia/2)+slotDia, sideOffset+constructLength) * millimeter
        });
    } 
