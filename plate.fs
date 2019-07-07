# **************************************************************************** #
#                                                                              #
#                                                         :::      ::::::::    #
#    plate.fs                                           :+:      :+:    :+:    #
#                                                     +:+ +:+         +:+      #
#    By: phtruong <marvin@42.fr>                    +#+  +:+       +#+         #
#                                                 +#+#+#+#+#+   +#+            #
#    Created: 2019/07/07 13:27:00 by phtruong          #+#    #+#              #
#    Updated: 2019/07/07 13:27:23 by phtruong         ###   ########.fr        #
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
        // Define the function's action
        // First extract numbers from input dimensions
        var width is number = definition.width / millimeter;
        var offset is number = definition.mirrorOffset / millimeter;
        var side_offset is number = definition.slotFromSide / millimeter;
        var slot_dia is number = definition.slotDia / millimeter;
        var slot1_offset is number = definition.slot1 / millimeter;
        var slot2_offset is number = definition.slot2 / millimeter;
        var slot3_offset is number = definition.slot3 / millimeter;
        // Set global variables for access
        setVariable(context, "b_width", width);
        setVariable(context, "offset", offset);
        setVariable(context, "side_offset", side_offset);
        setVariable(context, "slot_dia", slot_dia);
        setVariable(context, "slot1_off", slot1_offset);
        setVariable(context, "slot2_off", slot2_offset);
        setVariable(context, "slot3_off", slot3_offset);
        //Sketch the main image
        sketchBase(context, id);
    });

    /*
    ** function sketchBase:
    ** Recreate the 2D sketch in FeatureScript for faster changing dimension of the sketch via user inputs.
    ** Default unit: millimeter
    ** Functionality: Changes in FeatureScript allows faster visualization than changing variables in normal sketch
    */
    function sketchBase(context is Context, id is Id)
    {
        var base_sketch is Sketch = newSketch(context, id + "base", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        var width is number = getVariable(context, "b_width");
        var offset is number = getVariable(context, "offset");
        // By dividing the offset (distance between the 45 degrees center and the tangent offset) to sin(45), we can get the x distance from the vertex.
        var offset_x is number = offset/sin(45 * degree);
        // This is the side_offset for the slots from the side
        var side_offset is number = getVariable(context, "side_offset");
        var slot_dia is number = getVariable(context, "slot_dia");
        // Slot distances from one another, will break the image if the diameter is more than the offset.
        var slot1_offset = getVariable(context, "slot1_off");
        var slot2_offset = slot1_offset+getVariable(context, "slot2_off");
        var slot3_offset = slot2_offset+getVariable(context, "slot3_off");
        // Calculation for tangent constraints of the slot to the mirror offset, it's critical the find the construction line lengths, since there are currently no documentions on how to use skConstraint properly.
        var slot1_construct_length is number = width - (slot1_offset + (offset_x) + ((slot_dia/2)/sin(45 * degree)) + side_offset);
        var slot2_construct_length is number = width - (slot2_offset + (offset_x) + ((slot_dia/2)/sin(45 * degree)) + side_offset);
        var slot3_construct_length is number = width - (slot3_offset + (offset_x) + ((slot_dia/2)/sin(45 * degree)) + side_offset);
        
        // Sketch the base and contructions for the slots
        skRectangle(base_sketch, "base", {
                "firstCorner" : vector(0, 0) * millimeter,
                "secondCorner" : vector(width, width) * millimeter
        });
        skLineSegment(base_sketch, "mirror", {
                "start" : vector(0, width) * millimeter,
                "end" : vector(width, 0) * millimeter,
                "construction": true
        });
        skLineSegment(base_sketch, "offset", {
                "start" : vector(offset_x, width) * millimeter,
                "end" : vector(width, offset_x) * millimeter,
                "construction" : true
        });
        skLineSegment(base_sketch, "first_slot_construction", {
                "start" : vector(width-side_offset, width-slot1_offset) * millimeter,
                "end" : vector(width-side_offset-slot1_construct_length, width-slot1_offset) * millimeter,
                "construction" : true
        });
        skLineSegment(base_sketch, "second_slot_construction", {
                "start" : vector(width-side_offset, width-slot2_offset) * millimeter,
                "end" : vector(width-side_offset-slot2_construct_length, width-slot2_offset) * millimeter,
                "construction" : true
        });
        skLineSegment(base_sketch, "third_slot_construction", {
                "start" : vector(width-side_offset, width-slot3_offset) * millimeter,
                "end" : vector(width-side_offset-slot3_construct_length, width-slot3_offset) * millimeter,
                "construction" : true
        });
        
        // Sketch the lines then the arcs for each slot tangent to the mirror offset
        skLineSegment(base_sketch, "line1_bottom", {
                "start" : vector(width-side_offset, width-(slot1_offset-(slot_dia/2))) * millimeter,
                "end" : vector(width-side_offset-slot1_construct_length, width-(slot1_offset-(slot_dia/2))) * millimeter
        });
        skLineSegment(base_sketch, "line1_top", {
                "start" : vector(width-side_offset, width-(slot1_offset+(slot_dia/2))) * millimeter,
                "end" : vector(width-side_offset-slot1_construct_length, width-(slot1_offset+(slot_dia/2))) * millimeter
        });
        skArc(base_sketch, "slot1_arc_left", {
                "start" : vector(width-side_offset-slot1_construct_length, width-(slot1_offset-(slot_dia/2))) * millimeter,
                "mid" : vector(width-side_offset-(slot1_construct_length+(slot_dia/2)), width-slot1_offset) * millimeter,
                "end" : vector(width-side_offset-slot1_construct_length, width-(slot1_offset+(slot_dia/2))) * millimeter
        });
        skArc(base_sketch, "slot1_arc_right", {
                "start" : vector(width-side_offset, width-(slot1_offset-(slot_dia/2))) * millimeter,
                "mid" : vector(width-side_offset+(slot_dia/2), width-slot1_offset) * millimeter,
                "end" : vector(width-side_offset, width-(slot1_offset+(slot_dia/2))) * millimeter
        });
        skLineSegment(base_sketch, "line2_top", {
                "start" : vector(width-side_offset, width-slot2_offset+(slot_dia/2)) * millimeter,
                "end" : vector(width-side_offset-slot2_construct_length, width-slot2_offset+(slot_dia/2)) * millimeter
        });
        skLineSegment(base_sketch, "line2_bottom", {
                "start" : vector(width-side_offset, width-slot2_offset-(slot_dia/2)) * millimeter,
                "end" : vector(width-side_offset-slot2_construct_length, width-slot2_offset-(slot_dia/2)) * millimeter
        });
        skArc(base_sketch, "slot2_arc_left", {
                "start" : vector(width-side_offset-slot2_construct_length, width-slot2_offset-(slot_dia/2)) * millimeter,
                "mid" : vector(width-side_offset-(slot2_construct_length+(slot_dia/2)), width-slot2_offset) * millimeter,
                "end" : vector(width-side_offset-slot2_construct_length, width-slot2_offset+(slot_dia/2)) * millimeter
        });
        skArc(base_sketch, "slot2_arc_right", {
                "start" : vector(width-side_offset, width-slot2_offset-(slot_dia/2)) * millimeter,
                "mid" : vector(width-side_offset+(slot_dia/2), width-slot2_offset) * millimeter,
                "end" : vector(width-side_offset, width-slot2_offset+(slot_dia/2)) * millimeter
        });
        skLineSegment(base_sketch, "line3_top", {
                "start" : vector(width-side_offset, width-slot3_offset+(slot_dia/2)) * millimeter,
                "end" : vector(width-side_offset-slot3_construct_length, width-slot3_offset+(slot_dia/2)) * millimeter
        });
        skLineSegment(base_sketch, "line3_bottom", {
                "start" : vector(width-side_offset, width-slot3_offset-(slot_dia/2)) * millimeter,
                "end" : vector(width-side_offset-slot3_construct_length, width-slot3_offset-(slot_dia/2)) * millimeter
        });
        skArc(base_sketch, "slot3_arc_left", {
                "start" : vector(width-side_offset-slot3_construct_length, width-slot3_offset-(slot_dia/2)) * millimeter,
                "mid" : vector(width-side_offset-(slot3_construct_length+(slot_dia/2)), width-slot3_offset) * millimeter,
                "end" : vector(width-side_offset-slot3_construct_length, width-slot3_offset+(slot_dia/2)) * millimeter
        });
        skArc(base_sketch, "slot3_arc_right", {
                "start" : vector(width-side_offset, width-slot3_offset-(slot_dia/2)) * millimeter,
                "mid" : vector(width-side_offset+(slot_dia/2), width-slot3_offset) * millimeter,
                "end" : vector(width-side_offset, width-slot3_offset+(slot_dia/2)) * millimeter
        });
        // Here sketch the mirror slots
        skLineSegment(base_sketch, "line1_mirror_left", {
                "start" : vector(slot1_offset-(slot_dia/2), side_offset) * millimeter,
                "end" : vector(slot1_offset-(slot_dia/2), side_offset+slot1_construct_length) * millimeter
        });
        skLineSegment(base_sketch, "line1_mirror_right", {
                "start" : vector(slot1_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter,
                "end" : vector(slot1_offset-(slot_dia/2)+slot_dia, side_offset+slot1_construct_length) * millimeter
        });
        skArc(base_sketch, "slot1_arc_mirror_bot", {
                "start" : vector(slot1_offset-(slot_dia/2), side_offset) * millimeter,
                "mid" : vector(slot1_offset-(slot_dia/2)+(slot_dia/2), side_offset-(slot_dia/2)) * millimeter,
                "end" : vector(slot1_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter
        });
        skArc(base_sketch, "slot1_arc_mirror_top", {
                "start" : vector(slot1_offset-(slot_dia/2), side_offset+slot1_construct_length) * millimeter,
                "mid" : vector(slot1_offset-(slot_dia/2)+(slot_dia/2), side_offset+slot1_construct_length+(slot_dia/2)) * millimeter,
                "end" : vector(slot1_offset-(slot_dia/2)+slot_dia, side_offset+slot1_construct_length) * millimeter
        });
        skLineSegment(base_sketch, "line2_mirror_left", {
                "start" : vector(slot2_offset-(slot_dia/2), side_offset) * millimeter,
                "end" : vector(slot2_offset-(slot_dia/2), side_offset+slot2_construct_length) * millimeter
        });
        skLineSegment(base_sketch, "line2_mirror_right", {
                "start" : vector(slot2_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter,
                "end" : vector(slot2_offset-(slot_dia/2)+slot_dia, side_offset+slot2_construct_length) * millimeter
        });
        skArc(base_sketch, "slot2_arc_mirror_bot", {
                "start" : vector(slot2_offset-(slot_dia/2), side_offset) * millimeter,
                "mid" : vector(slot2_offset-(slot_dia/2)+(slot_dia/2), side_offset-(slot_dia/2)) * millimeter,
                "end" : vector(slot2_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter
        });
        skArc(base_sketch, "slot2_arc_mirror_top", {
                "start" : vector(slot2_offset-(slot_dia/2), side_offset+slot2_construct_length) * millimeter,
                "mid" : vector(slot2_offset-(slot_dia/2)+(slot_dia/2), side_offset+slot2_construct_length+(slot_dia/2)) * millimeter,
                "end" : vector(slot2_offset-(slot_dia/2)+slot_dia, side_offset+slot2_construct_length) * millimeter
        });
        skLineSegment(base_sketch, "line3_mirror_left", {
                "start" : vector(slot3_offset-(slot_dia/2), side_offset) * millimeter,
                "end" : vector(slot3_offset-(slot_dia/2), side_offset+slot3_construct_length) * millimeter
        });
        skLineSegment(base_sketch, "line3_mirror_right", {
                "start" : vector(slot3_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter,
                "end" : vector(slot3_offset-(slot_dia/2)+slot_dia, side_offset+slot3_construct_length) * millimeter
        });
        skArc(base_sketch, "slot3_arc_mirror_bot", {
                "start" : vector(slot3_offset-(slot_dia/2), side_offset) * millimeter,
                "mid" : vector(slot3_offset-(slot_dia/2)+(slot_dia/2), side_offset-(slot_dia/2)) * millimeter,
                "end" : vector(slot3_offset-(slot_dia/2)+slot_dia, side_offset) * millimeter
        });
        skArc(base_sketch, "slot3_arc_mirror_top", {
                "start" : vector(slot3_offset-(slot_dia/2), side_offset+slot3_construct_length) * millimeter,
                "mid" : vector(slot3_offset-(slot_dia/2)+(slot_dia/2), side_offset+slot3_construct_length+(slot_dia/2)) * millimeter,
                "end" : vector(slot3_offset-(slot_dia/2)+slot_dia, side_offset+slot3_construct_length) * millimeter
        });
        skSolve(base_sketch);    
    } 
