function ConvertTo-ComparablePgDump {
    param([AllowEmptyCollection()][string[]]$Lines)
    $kept = foreach ($line in $Lines) {
        if ($line.StartsWith('--', [StringComparison]::Ordinal) -or
            $line.StartsWith('\restrict ', [StringComparison]::Ordinal) -or
            $line.StartsWith('\unrestrict ', [StringComparison]::Ordinal) -or
            [string]::IsNullOrWhiteSpace($line)) { continue }
        $line
    }
    return ($kept -join "`n")
}
