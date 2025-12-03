class_name Bitflags

static func cond(condition: bool, bits: int, mask: int) -> int:
	if condition:
		return bits | mask
	else:
		return bits & (~mask)
