using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

public class SaveData
{
    public int Map1_Completed;  // Map 1 completion status
    public int Map2_Completed;  // Map 2 completion status
}

public static class SaveManager
{
    private static string key = "your-256-bit-long-encryption-key";  // 32-byte key (256 bits)
    private static string iv = "your-encryption-";    // 16-byte (128-bit) IV


    // Save progress to an encrypted file
    public static void SaveProgress(string selectedMap)
    {
        Debug.Log("Key Length: " + key.Length);  // Should be 16 bytes
        Debug.Log("IV Length: " + iv.Length);    // Should be 16 bytes

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "saveData.json");

        // Create directory if it doesn't exist
        if (!Directory.Exists(Directory.GetCurrentDirectory()))
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory());
        }

        // Get the level completion status (default is 0)
        int mapCompleted = 0;  // Default not completed
        if (selectedMap == "Map1") mapCompleted = 1; // Assume Map1 is completed

        // Create save data (using SaveData)
        SaveData saveData = new SaveData();
        if (File.Exists(filePath))
        {
            string newEncryptedData = File.ReadAllText(filePath);
            string json = Decrypt(newEncryptedData);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        if (selectedMap == "Map1") saveData.Map1_Completed = 1;
        else if (selectedMap == "Map2") saveData.Map2_Completed = 1;

        // Convert data to JSON format
        string saveDataJson = JsonUtility.ToJson(saveData);

        // Encrypt the save data
        string encryptedData = Encrypt(saveDataJson);

        // Write the encrypted data to the file
        File.WriteAllText(filePath, encryptedData);
        Debug.Log("Progress saved to file (encrypted).");
    }

    // Encryption method (AES)
    private static string Encrypt(string data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);  // Use UTF8 key
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);    // Use UTF8 initialization vector

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);  // Write the data
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());  // Return the encrypted data
            }
        }
    }

    // Load progress from the encrypted file
    public static void LoadProgress(string selectedMap)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "saveData.json");

        if (File.Exists(filePath))
        {
            try
            {
                // Read the encrypted file content
                string encryptedData = File.ReadAllText(filePath);

                // Decrypt the data
                string json = Decrypt(encryptedData);
                Debug.Log(json);
                // Parse the JSON data and update the status
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Check if necessary data is included
                if (data == null || (selectedMap == "Map1" && data.Map1_Completed == 0) || (selectedMap == "Map2" && data.Map2_Completed == 0))
                {
                    Debug.LogError("The save file is corrupted or invalid!");
                    return;
                }

                // Load progress based on selected map
                if (selectedMap == "Map1")
                    PlayerPrefs.SetInt("Map1_Completed", data.Map1_Completed);  // Set the completion status of that map
                if (selectedMap == "Map2")
                    PlayerPrefs.SetInt("Map2_Completed", data.Map2_Completed);  // Set the completion status of that map

                PlayerPrefs.Save();  // Save the changes

                Debug.Log("Progress loaded from file (decrypted).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load save file. The file might be corrupted or tampered with.");
                Debug.LogError("Error: " + ex.Message);
            }
        }
        else
        {
            // If the file doesn't exist, set the default progress as not completed (all maps set to 0)
            Debug.Log("No save file found, starting with default progress.");
            PlayerPrefs.SetInt(selectedMap + "_Completed", 0);  // Default map not completed
            PlayerPrefs.Save();
        }
    }

    // Decryption method (AES)
    public static string Decrypt(string encryptedData)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);  // Use UTF8 key
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);    // Use UTF8 initialization vector

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();  // Return the decrypted data
                    }
                }
            }
        }
    }
}
