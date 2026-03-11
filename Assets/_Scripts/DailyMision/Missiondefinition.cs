using UnityEngine;

/// <summary>
/// Tipos de misión disponibles. Agrega más aquí en el futuro.
/// </summary>
public enum MissionType
{
    CollectCoins,       // Recolectar X monedas en una sesión
    TravelDistance,     // Recorrer X metros en una sesión
    HelpPeople         // Ayudar X personas en una sesión
}

/// <summary>
/// Define los datos de una misión individual.
/// Crea instancias de esto en DailyMissionManager.allPossibleMissions.
/// </summary>
[System.Serializable]
public class MissionDefinition
{
    [Tooltip("Tipo de misión")]
    public MissionType type;

    [Tooltip("Descripción que verá el jugador. Usa {0} para el objetivo. Ej: 'Recolecta {0} monedas'")]
    public string descriptionTemplate;

    [Tooltip("Objetivo numérico a alcanzar")]
    public int goal;

    [Tooltip("Monedas que se otorgan al completar")]
    public int rewardCoins;
}