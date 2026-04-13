# BudgetTracker

Application mobile et de bureau pour la gestion de budget personnel, développée avec .NET MAUI.

## Fonctionnalités

- Suivi des revenus et des dépenses
- Gestion des catégories de budget avec indicateurs visuels
- Historique des transactions avec recherche
- Statistiques mensuelles par catégorie
- Gestion du profil et changement de mot de passe
- Compte de démonstration avec données préchargées

## Technologies

- .NET 9.0 MAUI (iOS, Android, Windows, macOS)
- SQLite pour la persistance locale
- BCrypt pour le hachage des mots de passe
- CommunityToolkit.Maui

## Prérequis

- Visual Studio 2022 avec la charge de travail .NET MAUI
- .NET 9.0 SDK

## Démarrage

1. Cloner le dépôt
2. Ouvrir `BudgetTracker.sln` dans Visual Studio
3. Sélectionner la plateforme cible (Android, iOS, Windows...)
4. Lancer le projet

Pour tester rapidement, utiliser le compte de démonstration disponible sur l'écran de connexion.

## Structure du projet

```
BudgetTracker/
├── Models/          # User, Transaction, BudgetCategory
├── Services/        # DatabaseService, AuthService, UserSession
├── ViewModels/      # Un ViewModel par page (pattern MVVM)
├── Views/           # Pages XAML
├── Converters/      # Convertisseurs de valeurs pour les liaisons
└── Resources/       # Polices, icônes, styles
```

## Auteurs

- Pierre
- Aboubacar
