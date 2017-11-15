﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Agent))]
public class AgentEditor : Editor
{
    private Material material;
    private Dictionary<InterestCategory, float[]> AttractednessBuffer = new Dictionary<InterestCategory, float[]>();
    private Dictionary<InterestCategory, float[]> AttractivenessBuffer = new Dictionary<InterestCategory, float[]>();
    private Dictionary<InterestCategory, float[]> PersonalAttractionBuffer = new Dictionary<InterestCategory, float[]>();
    public const int _bufferSize = 100;
    private int _arrayPointer = 0;
    private Agent _agent;

    void OnEnable()
    {
        _agent = (Agent)target;
        material = new Material(Shader.Find("Hidden/Internal-Colored"));

        InitArrays();
        AddValueToBuffer(_agent._currentInterests);
    }

    void InitArrays()
    {
        foreach (KeyValuePair<InterestCategory, float> pair in _agent._currentInterests)
        {
            AttractednessBuffer[pair.Key] = new float[_bufferSize];
            for (int i = 0; i < _bufferSize; i++)
                AttractednessBuffer[pair.Key][i] = 0f;

            AttractivenessBuffer[pair.Key] = new float[_bufferSize];
            for (int i = 0; i < _bufferSize; i++)
                AttractivenessBuffer[pair.Key][i] = 0f;

            PersonalAttractionBuffer[pair.Key] = new float[_bufferSize];
            for (int i = 0; i < _bufferSize; i++)
                PersonalAttractionBuffer[pair.Key][i] = 0f;
        }
    }

    public void AddValueToBuffer(Interests attractedness)
    {
        foreach (KeyValuePair<InterestCategory, float> pair in _agent._currentInterests)
        {
            var attraction = _agent.GetMostVisibilePointOfInterest(pair.Key);

            var foundAttraction = attraction != null;
            var generalAttraction = foundAttraction
            ? attraction.GetVisibilityAtGlobalPosition(_agent.transform.position)
            : 0f;

            var personalAttraction = generalAttraction * _agent.GetCurrentInterest(pair.Key);

            AttractednessBuffer[pair.Key][_arrayPointer] = pair.Value;
            AttractivenessBuffer[pair.Key][_arrayPointer] = generalAttraction;
            PersonalAttractionBuffer[pair.Key][_arrayPointer] = personalAttraction;
        }


        _arrayPointer = (_arrayPointer + 1) % _bufferSize;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AddValueToBuffer(_agent._currentInterests);
        Rect _layoutRectangle = GUILayoutUtility.GetRect(10, 10000, 200, 200);
        GUILayoutUtility.GetRect(10, 10000, 4, 4);
        Rect _attractivenessRect = GUILayoutUtility.GetRect(10, 10000, 200, 200);
        GUILayoutUtility.GetRect(10, 10000, 4, 4);
        Rect _personalAttractionRect = GUILayoutUtility.GetRect(10, 10000, 200, 200);

        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(_layoutRectangle);
            GL.PushMatrix();
            GL.Clear(true, false, Color.black);
            material.SetPass(0);
            RenderBackgroundGrid(_layoutRectangle);
            RenderBuffer(_layoutRectangle, AttractednessBuffer);
            GL.PopMatrix();
            GUI.EndClip();

            GUI.BeginClip(_attractivenessRect);
            GL.PushMatrix();
            GL.Clear(true, false, Color.black);
            material.SetPass(0);
            RenderBackgroundGrid(_attractivenessRect);
            RenderBuffer(_attractivenessRect, AttractivenessBuffer);
            GL.PopMatrix();
            GUI.EndClip();

            GUI.BeginClip(_personalAttractionRect);
            GL.PushMatrix();
            GL.Clear(true, false, Color.black);
            material.SetPass(0);
            RenderBackgroundGrid(_personalAttractionRect);
            RenderBuffer(_personalAttractionRect, PersonalAttractionBuffer);
            GL.PopMatrix();
            GUI.EndClip();

        }

        GUILayout.EndHorizontal();
    }

    void RenderBackgroundGrid(Rect rect)
    {

        GL.Begin(GL.QUADS);
        GL.Color(Color.black);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(rect.width, 0, 0);
        GL.Vertex3(rect.width, rect.height, 0);
        GL.Vertex3(0, rect.height, 0);
        GL.End();

        GL.Begin(GL.LINES);

        int offset = (Time.frameCount * 2) % 50;
        int count = (int)(rect.width / 10) + 20;

        for (int i = 0; i < count; i++)
        {
            var isMajorSegment = (i % 5 == 0);
            Color lineColour = isMajorSegment
                ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.02f, 0.02f, 0.02f);
            GL.Color(lineColour);

            float x = i * 10 - offset;

            var xIsWithinBoundsOfRectangle = x >= 0 && x < rect.width;
            if (xIsWithinBoundsOfRectangle)
            {
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, rect.height, 0);
            }

            if (i < rect.height / 10)
            {
                GL.Vertex3(0, i * 10, 0);
                GL.Vertex3(rect.width, i * 10, 0);
            }
        }

        GL.End();
    }

    void RenderBuffer(Rect r, Dictionary<InterestCategory, float[]> dictionary)
    {
        foreach (KeyValuePair<InterestCategory, float[]> pair in dictionary)
        {
            GL.Begin(GL.LINES);
            GL.Color(pair.Key.Color);

            var buffer = pair.Value;
            for (int i = 0; i < _bufferSize - 1; i++)
            {
                var x0 = i * r.width / _bufferSize;
                var x1 = (i + 1) * r.width / _bufferSize;

                var y0 = r.height * (1 - buffer[(i + _arrayPointer) % _bufferSize]);
                var y1 = r.height * (1 - buffer[(i + _arrayPointer + 1) % _bufferSize]);

                GL.Vertex3(x0, y0, 0);
                GL.Vertex3(x1, y1, 0);
            }
        }
        GL.End();
    }
}