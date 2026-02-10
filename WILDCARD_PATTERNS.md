# Wildcard Patterns - Nouvelle Fonctionnalité

## 📌 Aperçu

La fonctionnalité **BlockedWildcardPatterns** permet de bloquer des domaines en utilisant des patterns avec wildcards (`*`), offrant un contrôle précis sur les variations de domaines tout en spécifiant le TLD exact.

## 🎯 Pourquoi cette fonctionnalité ?

### Problème résolu

Les spammeurs utilisent souvent des variations de domaines avec un pattern commun :
- `sdk0freegame.top`, `sdk3freegame.top`, `sdk7freegame.top`
- `crmsoftwareedge.com`, `crmsoftwarefocus.com`, `crmsoftwarepulse.com`
- `mycrmsoftware.com`, `testcrmsoftwaretools.com`

Avec les anciennes méthodes :
- ❌ **BlockedPatterns** avec `"crmsoftware"` bloque TOUS les TLDs (.com, .net, .org, etc.)
- ❌ **BlockedDomains** nécessite d'ajouter chaque variation individuellement

### Solution apportée

✅ **BlockedWildcardPatterns** permet de :
- Bloquer uniquement des TLDs spécifiques (ex: `.com` seulement)
- Utiliser des wildcards pour couvrir toutes les variations
- Maintenir une liste concise et maintenable

## 🔧 Utilisation

### Configuration

```csharp
app.UseReferrerBlock(options => 
{ 
    // Bloquer tous les domaines contenant "crmsoftware" avec .com uniquement
    options.BlockedWildcardPatterns.Add("*crmsoftware*.com");
    
    // Bloquer tous les domaines sdk[n]freegame.top
    options.BlockedWildcardPatterns.Add("sdk*freegame.top");
    
    // Bloquer tous les domaines contenant "spam" avec .net uniquement
    options.BlockedWildcardPatterns.Add("*spam*.net");
});
```

## 📊 Exemples de patterns

### Pattern: `*crmsoftware*.com`

| Domaine | Résultat | Raison |
|---------|----------|---------|
| `crmsoftwareedge.com` | ✅ Bloqué | Match le pattern |
| `mycrmsoftware.com` | ✅ Bloqué | Match le pattern |
| `crmsoftwarehub.com` | ✅ Bloqué | Match le pattern |
| `testcrmsoftwaretools.com` | ✅ Bloqué | Match le pattern |
| `crmsoftwareedge.net` | ❌ Autorisé | TLD différent (.net) |
| `mycrmtools.com` | ❌ Autorisé | Ne contient pas "crmsoftware" |

### Pattern: `sdk*freegame.top`

| Domaine | Résultat | Raison |
|---------|----------|---------|
| `sdk0freegame.top` | ✅ Bloqué | Match le pattern |
| `sdk7freegame.top` | ✅ Bloqué | Match le pattern |
| `sdk123freegame.top` | ✅ Bloqué | Match le pattern |
| `sdkanyfreegame.top` | ✅ Bloqué | Match le pattern |
| `sdk0freegame.com` | ❌ Autorisé | TLD différent (.com) |
| `freegame.top` | ❌ Autorisé | Ne commence pas par "sdk" |

### Pattern: `*spam*.net`

| Domaine | Résultat | Raison |
|---------|----------|---------|
| `spam.net` | ✅ Bloqué | Match le pattern |
| `myspamsite.net` | ✅ Bloqué | Match le pattern |
| `spamnetwork.net` | ✅ Bloqué | Match le pattern |
| `test-spam-tools.net` | ✅ Bloqué | Match le pattern |
| `spam.com` | ❌ Autorisé | TLD différent (.com) |

## 🆚 Comparaison avec BlockedPatterns

### Avec BlockedPatterns (ancien)

```csharp
options.BlockedPatterns.Add("crmsoftware");
```

**Bloque :**
- ✅ `crmsoftwareedge.com`
- ✅ `crmsoftwareedge.net` ← **Peut-être non souhaité**
- ✅ `crmsoftwareedge.org` ← **Peut-être non souhaité**
- ✅ `mycrmsoftware.io` ← **Peut-être non souhaité**

**Problème :** Bloque TOUS les TLDs sans distinction.

### Avec BlockedWildcardPatterns (nouveau)

```csharp
options.BlockedWildcardPatterns.Add("*crmsoftware*.com");
```

**Bloque :**
- ✅ `crmsoftwareedge.com`
- ✅ `mycrmsoftware.com`
- ✅ `testcrmsoftwaretools.com`

**Autorise :**
- ❌ `crmsoftwareedge.net` ← **Contrôle précis**
- ❌ `crmsoftwareedge.org` ← **Contrôle précis**
- ❌ `mycrmsoftware.io` ← **Contrôle précis**

## ⚙️ Implémentation technique

### Algorithme

L'algorithme de matching utilise une approche **greedy backtracking** :
1. Parcourt le domaine et le pattern caractère par caractère
2. Quand `*` est rencontré, mémorise la position pour backtracking
3. Compare les caractères de manière case-insensitive
4. Si mismatch, retourne au dernier `*` et essaie la prochaine position

### Performance

- ⚡ Optimisé avec `ReadOnlySpan<char>` (zéro allocation)
- 🔥 Complexité O(n*m) dans le pire cas (avec backtracking)
- ✅ Optimisé pour les cas communs (peu de wildcards)

## 🧪 Tests unitaires

15 tests unitaires couvrent cette fonctionnalité :

### Tests de blocage
- ✅ Patterns avec suffix (`crmsoftwareedge.com`)
- ✅ Patterns avec prefix (`mycrmsoftware.com`)
- ✅ Patterns avec les deux (`mycrmsoftwarehub.com`)
- ✅ Multiples variations CRM
- ✅ Multiples variations SDK
- ✅ Case insensitive
- ✅ Avec subdomains
- ✅ Avec path
- ✅ Avec port

### Tests de non-blocage
- ✅ TLD différent
- ✅ Match partiel
- ✅ Pas de match
- ✅ Options vides
- ✅ Collection null

## 📈 Exemples réels de spam

### Pattern: `*crmsoftware*.com`

Bloque automatiquement :
```
crmsoftwareedge.com
crmsoftwarefocus.com
crmsoftwarepulse.com
crmsoftwareradar.com
crmsoftwarespotlight.com
mycrmsoftwarehub.com
testcrmsoftwaretools.com
```

### Pattern: `sdk*freegame.top`

Bloque automatiquement :
```
sdk0freegame.top
sdk3freegame.top
sdk7freegame.top
sdk123freegame.top
sdkanyfreegame.top
```

## 🔄 Migration depuis BlockedPatterns

Si vous utilisiez déjà `BlockedPatterns` :

### Avant (trop large)
```csharp
options.BlockedPatterns.Add("crmsoftware"); // Bloque TOUS les TLDs
```

### Après (précis)
```csharp
options.BlockedWildcardPatterns.Add("*crmsoftware*.com"); // Bloque uniquement .com
options.BlockedWildcardPatterns.Add("*crmsoftware*.net"); // Bloque uniquement .net si nécessaire
```

## 🎓 Bonnes pratiques

1. **Soyez spécifique avec les TLDs**
   - ✅ `"*spam*.com"` - Précis
   - ❌ `"spam"` (BlockedPatterns) - Trop large

2. **Utilisez des wildcards judicieusement**
   - ✅ `"sdk*freegame.top"` - Pattern clair
   - ❌ `"*spam*"` sans TLD - Utiliser BlockedPatterns à la place

3. **Testez vos patterns**
   - Vérifiez qu'ils ne bloquent pas de domaines légitimes
   - Ajoutez des tests unitaires pour vos patterns custom

4. **Documentez vos patterns**
   - Expliquez pourquoi chaque pattern est ajouté
   - Mentionnez des exemples de domaines bloqués

## 🚀 Conclusion

La fonctionnalité **BlockedWildcardPatterns** offre un contrôle précis sur le blocage de domaines spam tout en maintenant une configuration simple et maintenable. Elle complète parfaitement les autres méthodes de blocage (TLDs, domaines, patterns simples, préfixes de sous-domaines).
