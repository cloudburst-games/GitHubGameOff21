using Godot;
using System;

[Serializable()]
public class ExperienceManager
{
    public float GetTotalExperienceValueOfLevel(int level)
	{

		return level == 1 ? 0 : (float) Math.Round((5* Math.Pow(level, 3)) / 4);
	}

    public float GetTotalExperienceFromVictory(float experienceValueOfDefeated, int numOfDefeated, int numOfCompanions)
    {
        // add 5 xp per defeated opponent
        experienceValueOfDefeated += numOfDefeated*5;
        // party bonus
        for (int i = 0; i < numOfCompanions; i++)
        {
            experienceValueOfDefeated*=1.1f;
        }
        // divide by 1.5
        return (float)Math.Ceiling(experienceValueOfDefeated/1.5f);
    }

    public float GetExperienceNeededForNextLevel(int currentLevel)
    {
        return GetTotalExperienceValueOfLevel(currentLevel+1) - GetTotalExperienceValueOfLevel(currentLevel);
    }

    public float GetExperienceSinceCurrentLevel(int currentLevel, float totalExperience)
    {
        return totalExperience - GetTotalExperienceValueOfLevel(currentLevel);
    }

    public bool CanLevelUp(int currentLevel, float totalExperience)
    {   
        if (GetExperienceSinceCurrentLevel(currentLevel, totalExperience) >= GetExperienceNeededForNextLevel(currentLevel))
        {
            return true;
        }
        return false;
    }
}
