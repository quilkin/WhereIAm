var accumulated_output_info;

// add a labelled value to the text area
function accumulate_output(str) {
    accumulated_output_info = accumulated_output_info + str + "\n";
}

//-------------------- big integer class and routines -----------------------
// These routines do one decimal digit per array entry
// Not terribly fast, but so far everything else I've tried has been worse

// compare the absolute value of two bigints
// return -1 if this is larger, 0 if equal, and -1 if cmpval is larger
function bigint_absCmp_internal(firstval, secondval) {
    var i;
    //window.alert("abs_cmp starting" );
    // does it have more digits (-->bigger)?
    if (firstval.length > secondval.length) {
        //window.alert("absCmp, first number longer");
        return +1;
    }

    // does it have fewer digits (--> smaller)
    if (firstval.length < secondval.length) {
        //window.alert("absCmp, second number longer");
        return -1;
    }

    // equal number of digits, have to actually check the digits
    for (i = firstval.length - 1; i >= 0; i--) {
        //window.alert("abs_cmp, checking digit " + i );
        // does this digit resolve it?
        if (firstval.digits[i] > secondval.digits[i]) {
            //window.alert("absCmp, first number: digit " + i + " is bigger");
            return +1;
        }
        if (firstval.digits[i] < secondval.digits[i]) {
            //window.alert("absCmp, second number: digit " + i + " is bigger");
            return -1;
        }
    }

    // if we get here, size and digits are equal
    //window.alert("abs_cmp, numbers equal");
    return 0;
}

// compare a bigint to a normal integer
// return -1 if bigint is larger, 0 if equal, and -1 if normal int is larger
function bigint_sCmp_internal(cmpval) {
    var i;
    var curval = cmpval;

    // is the bigint too big for the code below to work?
    if (this.length > 10) {
        if (this.isneg)
            return -1;				// large negative
        else
            return +1;				// large positive
    }

    // convert bigint to normal int and compare
    var big_val = this.toNumber();
    //window.alert("sCmp: num=" + this.toString() + ", big_val=" + big_val );
    if (big_val < cmpval)
        return -1;
    else if (big_val > cmpval)
        return +1;
    else
        return 0;
}

// compare two bigints, return TRUE if they are equal
// return -1 if this is larger, 0 if equal, and -1 if cmpval is larger
function bigint_cmp_internal(cmpval) {
    var i;

    //window.alert("starting cmp_internal");
    // check signs (note: this doesn't account for -0)
    if (this.isneg != cmpval.isneg) {
        // negative = smaller
        if (this.isneg)
            return -1;				// this negative, cmpval positive
        else
            return 1;				// this positive, cmpval negative
    }

    //window.alert("cmp_internal, same sign");
    // compare absolute values
    var abs_cmp = bigint_absCmp_internal(this, cmpval);

    //window.alert("cmp_internal, absCmp returned " + abs_cmp );
    // reverse it if negative
    if (this.isneg)
        return -abs_cmp;
    else
        return abs_cmp;				// positive
}

// is this number 0?
function bigint_isZero_internal() {
    if (this.length > 1)
        return false;			// 2+ digits, normalized number isn't 0

    // one digit - check it
    return this.digits[0] == 0;
}

// clear a bigint to 0
function bigint_clear_internal() {
    this.isneg = false;
    this.length = 1;
    while (this.digits.length > 1)
        this.digits.pop();
    this.digits[0] = 0;
}

// copy a normal integer into a bigint
function bigint_setInt_internal(newval) {
    this.clear();

    this.isneg = false;				// assume newval is positive
    if (newval < 0) {
        this.isneg = true;
        newval = -newval;
    }

    // divide the number up
    while (newval > 10) {
        this.digits.push(newval % 10);
        newval = Math.floor(newval / 10);
    }

    // add final digit (or only 0 )
    this.digits.push(newval);

    // remove the 0 left by clear()
    this.digits.shift();

    // clean up and finish
    this.normalize();
}

// copy a bigint
function bigint_copyNum_internal(newval) {
    var i;
    //window.alert("copyNum: starting");
    // erase any existing value
    this.clear();

    // copy values from the other value
    this.isneg = newval.isneg;
    this.length = newval.length;
    //window.alert("copyNum: length=" + this.length );
    for (i = 0; i < newval.length; i++)
        this.digits[i] = newval.digits[i];
}

// parse a string into a bigint
function bigint_parseInt_internal(strval) {
    var pos;

    this.clear();

    // is this positive or negative?
    if (strval.charAt(0) == "-") {  // mark this as negative
        this.isneg = true;
        // strip the '-' from the number
        strval = strval.substr(1);
    }

    // process digits starting from the bottom
    for (pos = 0; pos < strval.length; pos++) {
        // get one digit, convert to integer
        this.digits[pos] = strval.charAt(strval.length - pos - 1) - 0;
    }
    this.length = strval.length;
    this.normalize();
}

// generate a random positive integer with the appropriate # of digits
function bigint_genRand_internal(numdig) {
    var i;

    // erase any current number
    this.clear();

    // create the digits
    for (i = 0; i < numdig - 1; i++) {
        this.digits[i] = Math.floor(Math.random() * 10);
    }
    // require the most significant digit not be 0
    this.digits[numdig - 1] = 1 + Math.floor(Math.random() * 9);

    //window.alert( "genRand numdig=" + numdig + ", result (reversed) " + this.digits.join() );
    // fix length, handle leading 0's
    this.normalize();
}

// "normalize" a number
// - remove leading 0's
// - set this.length
// - check for -0
function bigint_normalize_internal() {
    // remove any extra 0 entries at the end of the digits array
    while (this.digits.length > 1 && this.digits[this.digits.length - 1] == 0) {
        // remove leading 0
        //window.alert("normalize: removing leading 0");
        this.digits.pop();
        //window.alert("normalize: new length: " + this.digits.length );
    }

    // note final length
    this.length = this.digits.length;

    // check for -0
    if (this.isneg && this.length == 1 && this.digits[0] == 0) {
        // have -0, change it to +0
        this.isneg = false;
    }
}

// make this bigint odd
function bigint_makeOdd_internal() {
    // set the bottom bit to 1
    this.digits[0] |= 1;
}

// convert this number into a string
function bigint_toString_internal() {
    var i;
    var str = "";

    if (this.isneg)
        str = "-";

    for (i = this.length - 1; i >= 0; i--)
        str = str + this.digits[i];		// note this implies int->char conversion

    return str;
}

// convert this to an ordinary number
// depending on size, it may convert to a float and lose some low-order digits
function bigint_toNumber_internal() {
    var i;
    var val = 0;

    for (i = this.length - 1; i >= 0; i--)
        val = (val * 10) + this.digits[i];

    // is it negative?
    if (this.isneg)
        // yes, return -x
        return -val;
    else
        // no, return x
        return val;
}

// "convert" the number to a digit
// if the bigint is between -9 and +9, return the value
//    else return +10 or -10
// this is faster than toNumber if we want to test for 1, 0, or -1
function bigint_toDigit_internal() {
    var lastdig;

    // do we have more than one digit in this number?
    if (this.length > 1)
        // more than one digit
        lastdig = 10;
    else
        // have one digit
        lastdig = this.digits[0];

    // is it negative?
    if (this.isneg)
        // yes, return -x
        return -lastdig;
    else
        // no, return x
        return lastdig;
}

// add a bigint to the current bigint
// know the signs are the same
function bigint_addBase_internal(firstval, secondval) {
    var result = new bigint();
    var temp;
    var carry = 0;
    var i;

    // set sign of the result
    result.isneg = firstval.isneg;

    // how long is the shorter array?
    var minlen = Math.min(firstval.length, secondval.length);

    // process common segment
    for (i = 0; i < minlen; i++) {
        // do the addition
        temp = firstval.digits[i] + secondval.digits[i] + carry;

        // separate digit and carry
        result.digits[i] = temp % 10;
        carry = Math.floor(temp / 10);
    }

    // process any extra in this array
    for (i = minlen; i < firstval.length; i++) {
        // do the addition
        temp = firstval.digits[i] + carry;

        // separate digit and carry
        result.digits[i] = temp % 10;
        carry = Math.floor(temp / 10);
    }

    // process any extra in the second array
    for (i = minlen; i < secondval.length; i++) {
        // do the addition
        temp = secondval.digits[i] + carry;

        // separate digit and carry
        result.digits[i] = temp % 10;
        carry = Math.floor(temp / 10);
    }

    // do we have a final carry?
    if (carry > 0)
        result.digits.push(carry);

    result.normalize();
    return result;
}

// subtract two bigints
// assumes we have a true subtract, with firstval > secondval
function bigint_subBase_internal(firstval, secondval) {
    var result = new bigint();
    var temp;
    var carry = 0;
    var i;

    // set sign of the result
    result.isneg = firstval.isneg;

    // process common segment
    for (i = 0; i < secondval.length; i++) {
        // do the addition
        temp = firstval.digits[i] - secondval.digits[i] + carry;

        // separate digit and carry
        if (temp >= 0) {
            result.digits[i] = temp;
            carry = 0;
        }
        else {
            result.digits[i] = temp + 10;
            carry = -1;
        }
    }

    // process any extra in firstval array
    for (i = secondval.length; i < firstval.length; i++) {
        // do the addition
        temp = firstval.digits[i] + carry;

        // separate digit and carry
        if (temp >= 0) {
            result.digits[i] = temp;
            carry = 0;
        }
        else {
            result.digits[i] = temp + 10;
            carry = -1;
        }
    }

    result.normalize();
    return result;
}

// add two numbers
function bigint_add_internal(secondval) {
    // is this really an add?
    if (this.isneg == secondval.isneg) {
        return bigint_addBase_internal(this, secondval);
    }

    // different signs, we are really subtracting
    // are we doing x-y with x bigger?
    if (bigint_absCmp_internal(this, secondval) >= 0) {
        // yes, we have x+(-y) with x larger
        return bigint_subBase_internal(this, secondval);
    }
    else {
        // no, we have x+(-y) with y larger
        var result;

        // so calculate -(|y|-|x|)
        result = bigint_subBase_internal(secondval, this);
        result.isneg = secondval.isneg;
        return result;
    }
}

// subtract two numbers
function bigint_sub_internal(secondval) {
    // is this really an add (-x)-y or x-(-y)?
    if (this.isneg != secondval.isneg) {
        return bigint_addBase_internal(this, secondval);
    }

    // same signs, we are really subtracting
    // are we doing x-y with x bigger?
    if (bigint_absCmp_internal(this, secondval) >= 0) {
        // yes, we have x-y with x larger
        return bigint_subBase_internal(this, secondval);
    }
    else {
        // no, we have x-y with y larger
        var result;

        // so calculate -(y-x)
        result = bigint_subBase_internal(secondval, this);
        result.isneg = !result.isneg;
        return result;
    }
}

// multiply two (positive??) bigints
function bigint_mul_internal(secondval) {
    var result = new bigint();
    var i;
    var j;
    var temp;
    var carry;

    //window.alert("mul: starting");
    // note the sign of the result
    if (this.isneg)
        result.isneg = !secondval.isneg;		// have (-x)*y
    else
        result.isneg = secondval.isneg;		// have (+x)*y

    // set the result to 0 for now
    for (i = 0; i < this.length + secondval.length; i++)
        result.digits[i] = 0;

    //window.alert("mul: multiplying digits");
    // do the multiplication
    for (i = 0; i < this.length; i++)
        for (j = 0; j < secondval.length; j++) {
            result.digits[i + j] += this.digits[i] * secondval.digits[j];
        }

    //window.alert("mul: processing carries");
    // process carries
    carry = 0;
    for (i = 0; i < result.digits.length; i++) {
        // do the addition
        temp = result.digits[i] + carry;

        // separate digit and carry
        result.digits[i] = temp % 10;
        carry = Math.floor(temp / 10);
    }

    //window.alert("mul: extending carry digits");
    // handle any additional carries
    while (carry > 0) {
        result.digits.push(carry % 10);
        carry = Math.floor(carry / 10);
    }

    //window.alert("mul: normalizing");
    result.normalize();
    return result;
}

// "shift" the current number "left" (to higher places) by a specified number of digits
function bigint_lshift_internal(shiftamt) {
    var i;

    // insert the appropriate number of 0 digits
    for (i = 0; i < shiftamt; i++)
        this.digits.unshift(0);

    this.length = this.digits.length;
}

// "shift" the current number "right" (to lower places) by a specified number of digits
function bigint_rshift_internal(shiftamt) {
    var i;

    // remove the appropriate number of digits
    for (i = 0; i < shiftamt; i++)
        this.digits.pop();

    this.length = this.digits.length;
}

// divide two bigints
// returns array with a[0] = quotient, a[1] = remainder
function bigint_divMod_internal(modval) {
    var tempnum = new bigint();
    var remainder = new bigint();
    var quotient = new bigint();
    remainder.copy(this);
    var shiftamt = this.length - modval.length;

    // note the sign of the result
    if (this.isneg)
        quotient.isneg = !modval.isneg;		// have (-x)*y
    else
        quotient.isneg = modval.isneg;		// have (+x)*y

    // process this digit by digit
    while (shiftamt >= 0) {
        // make sure we have a digit to use here
        quotient.digits[shiftamt] = 0;

        // shift the modulus left to fit the current value
        tempnum.copy(modval);
        tempnum.lshift(shiftamt);

        // subtract as long as _remainder_ >= _modval_
        while (bigint_absCmp_internal(remainder, tempnum) >= 0) {
            // subtract, count it off
            remainder = bigint_subBase_internal(remainder, tempnum);
            quotient.digits[shiftamt]++;
        }

        shiftamt--;
    }

    // clean up the numbers
    quotient.normalize();
    remainder.normalize();

    // returns array with a[0] = quotient, a[1] = remainder
    //var results = new Array();
    //results[0] = quotient;
    //results[1] = remainder;
    var results = new Array(quotient, remainder);
    return results;
}

// take the remainder of two bigints (quotient is ignored)
function bigint_mod_internal(modval) {
    var tempnum = new bigint();
    var remainder = new bigint();
    remainder.copy(this);
    var shiftamt = this.length - modval.length;

    // process this digit by digit
    while (shiftamt >= 0) {
        // shift the modulus left to fit the current value
        tempnum.copy(modval);
        tempnum.lshift(shiftamt);

        //		window.alert("mod: shiftamt=" + shiftamt + ", tempnum="+tempnum.toString() +
        //				", remainder=" + remainder.toString() );

        // subtract as long as _remainder_ >= _modval_
        while (bigint_absCmp_internal(remainder, tempnum) >= 0) {
            // subtract, count it off
            //			window.alert("mod: doing subBase");
            remainder = bigint_subBase_internal(remainder, tempnum);
            //			window.alert("mod: new remainder=" + remainder.toString() );
        }

        shiftamt--;
    }

    remainder.normalize();
    return remainder;
}

// do modular exponentiation
// take (this ** e) % n
// assumes all values are positive??
function bigint_expMod_internal(e, n, saveDetails) {
    var i;
    var outdata = new bigint();
    var indata = new bigint();
    var remexp = new bigint();		// remaining exponent from e
    var remexpdig;
    //window.alert("expMod starting" );
    remexp.copy(e);

    indata.copy(this);
    // set output to 1 before beginning multiplies
    outdata.setInt(1);
    if (saveDetails) accumulate_output("expMod: setting output to 1");

    // use square-and-multiply to handle exponent one bit at a time
    var modres = remexp.sDivMod(2);
    remexp = modres[0];			// quotient
    remexpdig = modres[1];		// remainder

    // are we done with the exponent bits
    while (remexp.toDigit() != 0 || remexpdig != 0) {
        //window.alert( "expMod: outdata="+outdata.toString() + ", remexp="+remexp.toString() );
        if (saveDetails) accumulate_output("expMod: last bit=" + remexpdig + ", remaining exponent=" + remexp.toString())
        if (remexpdig != 0) {
            // this bit of the exponent is 1, include it in outdata
            outdata = outdata.mul(indata).mod(n);
            if (saveDetails) accumulate_output("expMod: (out*msg) mod n=" + outdata.toString());
        }
        //window.alert( "expMod: new outdata=" + outdata.toString() );
        // square the data
        indata = indata.mul(indata).mod(n);
        if (saveDetails) accumulate_output("expMod: msg squared mod n=" + indata.toString());
        //window.alert( "expMod: new indata=" + indata.toString() );

        // get the next bit
        modres = remexp.sDivMod(2);
        remexp = modres[0];			// quotient
        remexpdig = modres[1];		// remainder
    }

    outdata.normalize();
    return outdata;
}

// add/subtract a normal integer to the current bigint
function bigint_sAdd_internal(val) {
    var result = new bigint();
    var temp;
    var carry = 0;
    var i;

    // be ready in case of a nontrivial negative number
    // don't expect this to be common
    if (this.isneg) {
        // use 'result' as the second value
        result.setInt(val);
        return this.add(result);
    }

    // separate add from true subtract
    if (val >= 0) {
        // note sign is already positive
        // process the addition
        carry = val;
        for (i = 0; i < this.length; i++) {
            // do the addition
            temp = this.digits[i] + carry;

            // separate digit and carry
            result.digits[i] = temp % 10;
            carry = Math.floor(temp / 10);
        }

        // do we have a final carry?
        while (carry > 0) {
            result.digits.push(carry);
            carry = Math.floor(carry / 10);
        }
    }
    else if (val >= -9) {
        carry = val;
        // will do single-digit subtracts
        for (i = 0; i < this.length; i++) {
            // do the addition
            temp = this.digits[i] + carry;

            // separate digit and carry
            if (temp >= 0) {
                result.digits[i] = temp;
                carry = 0;
            }
            else {
                result.digits[i] = temp + 10;
                carry = -1;
            }
        }
    }

    result.normalize();
    return result;
}

// return this * B + C
//   this: bigint
//   B, C: normal integers
// This will do the fast way if everything is positive
//   else will switch to bigint routines for everything
function bigint_sMulAdd_internal(mulval, addval) {
    var result = new bigint();
    var temp;
    var carry = 0;
    var i;

    // be ready in case of a negative number
    // don't expect this to be common
    if (this.isneg || mulval < 0 || addval < 0) {
        // do everything as bigints to be safe and easy
        var tempval = new bigint();

        // do the multiply
        tempval.setInt(mulval);
        tempval = this.mul(tempmul);

        // use 'result' as the second value
        result.setInt(val);
        return tempval.add(result);
    }

    // note sign is already positive
    // process the addition
    carry = addval;
    for (i = 0; i < this.length; i++) {
        // do the addition
        temp = this.digits[i] * mulval + carry;

        // separate digit and carry
        result.digits[i] = temp % 10;
        carry = Math.floor(temp / 10);
    }

    // do we have leftover digits?
    while (carry > 0) {
        temp = carry;
        // isolate the last digit
        result.digits.push(temp % 10);
        // remove it from the number
        carry = Math.floor(temp / 10);
    }

    // note length
    result.normalize();
    return result;
}

// divide a bigint by a normal integer
// returns array with a[0] = quotient (bigint), a[1] = remainder (integer)
function bigint_sDivMod_internal(modval) {
    var quotient = new bigint();
    var i;
    var temp;
    var remainder;

    // copy the quotient
    quotient.copy(this);
    remainder = 0;
    // process this one digit at a time
    for (i = this.length - 1; i >= 0; i--) {
        // what do we have, including carry from higher digits
        temp = quotient.digits[i] + remainder * 10;

        // get quotient, remainder for this digits
        quotient.digits[i] = Math.floor(temp / modval);
        remainder = temp % modval;
    }

    // normaize the remainder
    quotient.normalize();

    // returns array with a[0] = quotient, a[1] = remainder
    var results = Array();
    results[0] = quotient;
    results[1] = remainder;
    return results;
}

// take the remainder of an bigint divided by a normal integer
function bigint_sMod_internal(modval) {
    var i;
    var res;

    res = 0;
    // process this one digit at a time
    for (i = this.length - 1; i >= 0; i--) {
        res = (res * 10 + this.digits[i]) % modval;
    }

    return res;
}

// is this a "small" (32-bit integer) prime
// assume the number has already been checked and is odd
// note we do not use "this", but rely on a function parameter
function bigint_checkPrimeSmall_internal(testprime) {
    var i;		// counter
    var stopval;		// sqrt(this) for division testing

    // the numbers are "small"
    // we can do test division quick enough to be sure it is a prime
    // how far should we go to check if it is prime?
    stopval = Math.sqrt(testprime);

    // check divisors
    for (i = 3; i <= stopval; i += 2) {
        if ((testprime % i) == 0) {
            // found a divisor, so it isn't prime
            accumulate_output("Not prime: " + testprime + " is divisible by " + i);
            return false;
        }
    }

    // all tests succeeded, so know we have a prime
    return true;
}

function bigint_checkPrimeLarge_internal(chkprime) {
    var i;
    var a = new bigint();
    var remval;

    // is "large", so try a few divisions and then do probabilistic tests

    // do a bunch of trial divisions at once to save time
    // 4849845 = 3 * 5 * 7 * 11 * 13 * 17 * 19
    var ptemp = chkprime.sMod(4849845);

    if (ptemp % 3 == 0 || ptemp % 5 == 0 || ptemp % 7 == 0 || ptemp % 11 == 0 ||
               ptemp % 13 == 0 || ptemp % 17 == 0 || ptemp % 19 == 0) {
        accumulate_output("Fails small prime division tests");
        return false;
    }

    // move on to probabilistic tests
    var pm1 = chkprime.sAdd(-1);

    // 10 times should be enough for good confidence
    for (i = 0; i < 10; i++) {
        // try the test a**(-1) = 1 (mod p) for random a
        // use "length-1" digits to assure a<p
        a.genRand(chkprime.length - 1);
        //window.alert("checkPrimeLarge trying " + a.toString() + "**" + pm1.toString() + "  mod " + chkprime.toString() );

        // do the exponentiation
        accumulate_output("Checking " + a.toString() + " ** " + pm1.toString() + " mod " + chkprime.toString());
        remval = a.expMod(pm1, chkprime, false);
        //window.alert("checkPrimeLarge expMod result=" + remval );

        // did we succeed?
        if (remval.toDigit() != 1) {
            accumulate_output("Test failed");
            return false;
        }
    }

    // all tests succeeded, so assume we have a prime
    return true;
}

// full prime check
// never checks the sign, so should handle -3 as prime
function bigint_isPrime_internal() {
    // is it even?
    // check the final digit to find out
    var lastdig = this.digits[0];
    if (lastdig % 2 == 0) {
        // yes, but should also check if it is exactly 2
        if (this.toDigit() == 2)
            // yes, it is the prime 2
            return true;
        else
            // no, is is some other even number
            return false;
    }

    // is it "small" or "large"
    if (this.length <= 9)
        return bigint_checkPrimeSmall_internal(this.toNumber());
    else
        return bigint_checkPrimeLarge_internal(this);
}

//  given a number, find the next larger prime
function bigint_makePrime_internal() {
    // generate a new big integer for our number
    var chkprime = new bigint();
    var temp;
    chkprime.copy(this);
    accumulate_output("makePrime: got " + chkprime.toString());
    chkprime.makeOdd();

    // check if it is prime
    if (chkprime.length <= 9) {
        // convert this to a normal integer
        temp = chkprime.toNumber();
        accumulate_output("makePrime: Small prime: trying " + temp);
        //window.alert("makePrime: trying " + temp );
        // if not prime, move to the next larger odd number
        while (!bigint_checkPrimeSmall_internal(temp)) {
            // not prime, so try the next larger odd number
            temp += 2;
            accumulate_output("makePrime: Small prime: trying " + temp);
        }

        //window.alert( "makePrime: " + temp + " is a prime");
        // convert back to a bigint
        chkprime.setInt(temp);
    }
    else {
        accumulate_output("Large prime: trying " + chkprime.toString());
        // if not prime, move to the next larger odd number
        while (!bigint_checkPrimeLarge_internal(chkprime)) {
            //window.alert("makePrime: Trying large number " + chkprime.toString() );	  
            // not prime, so try the next larger odd number
            chkprime = chkprime.sAdd(2);
            accumulate_output("Large prime: trying " + chkprime.toString());
        }
    }
    //window.alert("makePrime: copying value " + chkprime.toString() );
    // copy our new prime
    this.copy(chkprime);
    //window.alert("makePrime: Copy done");
}

// generate a random prime with the specified number of digits
function bigint_genPrime_internal(numdig) {
    // generate a random number with the appropriate number of digits
    accumulate_output("Generating number with " + numdig + " digits");
    this.genRand(numdig);
    accumulate_output("Generated random number " + this.toString());

    // now make it into a prime
    this.makePrime();

    //window.alert("GenPrime - makePrime returned value");
    //window.alert("genPrime - final prime: "+this.toString() );
    accumulate_output("Final prime: " + this.toString());
}

// constructor function
function bigint() {
    // mark it as positive 0
    this.isneg = false;
    this.length = 1;
    this.digits = new Array(1);
    this.digits[0] = 0;

    // set up methods
    this.copy = bigint_copyNum_internal;
    this.clear = bigint_clear_internal;
    this.setInt = bigint_setInt_internal;
    this.parseInt = bigint_parseInt_internal;
    this.isPrime = bigint_isPrime_internal;
    this.genRand = bigint_genRand_internal;
    this.genPrime = bigint_genPrime_internal;
    this.makeOdd = bigint_makeOdd_internal;
    this.makePrime = bigint_makePrime_internal;
    this.toString = bigint_toString_internal;
    this.toNumber = bigint_toNumber_internal;
    this.toDigit = bigint_toDigit_internal;
    this.add = bigint_add_internal;
    this.sAdd = bigint_sAdd_internal;
    this.sub = bigint_sub_internal;
    this.mul = bigint_mul_internal;
    this.sMulAdd = bigint_sMulAdd_internal;
    this.divMod = bigint_divMod_internal;
    this.sDivMod = bigint_sDivMod_internal;
    this.mod = bigint_mod_internal;
    this.expMod = bigint_expMod_internal;
    this.sMod = bigint_sMod_internal;
    this.lshift = bigint_lshift_internal;
    //this.rshift = bigint_rshift_internal;
    this.cmp = bigint_cmp_internal;
    this.sCmp = bigint_sCmp_internal;
    this.cmpEQ = function (cmpval) { return this.cmp(cmpval) == 0; };
    //this.cmpNE = function( cmpval ) { return this.cmp(cmpval) != 0; };
    //this.cmpGT = function( cmpval ) { return this.cmp(cmpval) > 0; };
    //this.cmpGE = function( cmpval ) { return this.cmp(cmpval) >= 0; };
    //this.cmpLT = function( cmpval ) { return this.cmp(cmpval) < 0; };
    //this.cmpLE = function( cmpval ) { return this.cmp(cmpval) <= 0; };
    this.isZero = bigint_isZero_internal;
    this.isNegative = function () { return this.isneg; };
    this.normalize = bigint_normalize_internal;
}

//------------------ routines to get RSA data ------------------

// get the message to encrypt/decrypt as a decimal string
function get_message(typebox, str) {
    var i;
    var msg = new bigint();
    //	var str;

    // what type of data do we have to work with?
    if (typebox[0].checked) {
        // ASCII
        for (i = 0; i < str.length; i++)
            msg = msg.sMulAdd(256, str.charCodeAt(i));
    }
    else if (typebox[1].checked) {
        // hex
        for (i = 0; i < str.length; i++)
            msg = msg.sMulAdd(16, parseInt(str.charAt(i), 16));
    }
    else {
        // have decimal data
        msg.parseInt(str);
    }

    // message is ready to encrypt
    return msg;
}

// format a big integer as integer, ascii, or hex
function format_bigint(typebox, rawmsg) {
    var str = "";
    var digit;
    var split;

    // copy the message to avoid changing our saved output
    var msg = new bigint();
    msg.copy(rawmsg);

    // what type of data do we have to work with?
    if (typebox[0].checked) {
        // output ASCII data
        while (!msg.isZero()) {
            // convert the last character to ASCII
            split = msg.sDivMod(256);
            msg = split[0];                  // quotient
            str = String.fromCharCode(split[1]) + str;
        }
    }
    else if (typebox[1].checked) {
        // format hex data
        while (!msg.isZero()) {
            // convert the last character to ASCII
            split = msg.sDivMod(256);
            msg = split[0];                  // quotient
            str = split[1].toString(16) + str;
        }
    }
    else {
        // have decimal data
        str = msg.toString();
    }

    // ready to textbox
    return str;
}

// generate a pair of primes
function prime_gen() {
    // reset debug/detail output
    accumulated_output_info = "";

    // how many digits should p have??
    var psize = document.stuff.psize.options[document.stuff.psize.selectedIndex].text;
    //window.alert("Generating p, size=" + psize);

    // generate a random number of the right size
    // the Java BigInteger package has a generator, but it is bit-orientated,
    //	so it is awkward to guarantee a 4-digit number has 4 digits (not 3 or 5)
    var ptemp = new bigint();
    ptemp.genPrime(psize);
    document.stuff.primep.value = ptemp.toString();

    // how many digits should q have??
    var qsize = document.stuff.qsize.options[document.stuff.qsize.selectedIndex].text;
    //window.alert("Generating q, size=" + qsize);

    // generate a random number of the right size
    do {
        var qtemp = new bigint();
        qtemp.genPrime(qsize);
        //window.alert("generated prospective q");
        // make sure we haven't generated the same value
    } while (ptemp.cmpEQ(qtemp));
    //window.alert("Have q value");
    document.stuff.primeq.value = qtemp.toString();

    document.stuff.details.value = accumulated_output_info;
}

// determine if e is or is not relatively prime to (p-1)(q-1)
//   if not, return null
//   if so, return  the appropriate value of d
// Uses a variation on Euclid's GCD algorithm
//   Set x[0], x[1] = values to test
//       y[0]=0, y[1]=1
//       while ( x[i-1] > 0 )
//          quotient = floor( x[i-2] / x[i-1] );
//          x[i] = x[i-2] % x[i-1];
//          y[i] = y[i-2] - quotient*y[i-1];
// if x[i-2]==1, the values are relatively prime
//    in that case, y[i-1] is the decryption exponent
function check_exponent(p1q1, e) {
    //window.alert("check_exponent: p1q1=" + p1q1.toString() );
    var xm2 = new bigint();
    xm2.copy(p1q1);
    //window.alert("check_exponent: xm2=" + xm2.toString() );
    var xm1 = new bigint();
    xm1.copy(e);
    //window.alert("check_exponent: xm1=" + xm1.toString() );
    var x = new bigint();			// x[i-2], x[i-1], x[i]
    var ym2 = new bigint();
    //window.alert("t1");
    ym2.setInt(0);
    var ym1 = new bigint();
    ym1.setInt(1);
    //window.alert("t2");
    var y = new bigint();
    var results;
    var quotient;
    var i = 2;

    accumulate_output("x[0]=" + p1q1.toString() + ", y[0]=0");
    accumulate_output("x[1]=" + e.toString() + ", y[1]=1");

    // loop until x[i-2] is 0
    while (!xm1.isZero()) {
        //window.alert("check_exponent calling divMod");
        // calculate quotient and remainder
        results = xm2.divMod(xm1);
        //window.alert("divMod returned");
        // separate values in the array [quotient, remainder]
        quotient = results[0];
        x = results[1];

        // update calculations we will use for d
        y = ym2.sub(quotient.mul(ym1));
        //window.alert( "new values i="+i+": quotient="+quotient.toString()
        //				+", x[i]="+x.toString()+", y[i]="+y.toString() );

        accumulate_output("i=" + i + ": quotient=" + quotient.toString()
				+ ", x[i]=" + x.toString() + ", y[i]=" + y.toString());
        i++;

        // move to next round
        xm2 = xm1;
        xm1 = x;
        ym2 = ym1;
        ym1 = y;
    }
    //window.alert("Checking result xm2=" + xm2.toString() + ", ym2=" + ym2.toString() );
    // did we succeed or fail?
    if (xm2.sCmp(1) == 0) {
        // e is relatively prime to (p-1)(q-1)
        // note: y[i-2] may be negative! - take it "mod" p1q1
        if (ym2.isNegative())
            ym2 = ym2.add(p1q1);		// take result mod (p-1)(q-1)

        //accumulate_output( "succeeded, returning d="+ym2.toString() );
        return ym2;
    }
    else {
        accumulate_output("failure, not relatively prime to (p-1)(q-1)");
        return null;
    }
}

// given p, q, and e, calculate the decryption exponent d
//  -- may change e if necessary
// returns d
function calcDecryptExp(p, q, e) {
    var d;
    //window.alert("start calcDecryptExp");
    // did they want us to generate e? (is the supplied e = 0?)
    if (e.isZero()) {
        // yes, get a random odd value (>=3) for the exponent e
        e = new bigint();
        e.genPrime(Math.min(p.length - 2, 3));
    }

    // make sure our given or random number is odd
    e.makeOdd();
    //window.alert("calcDecryptExponent: modified e=" + e.toString() );

    var pm1 = p.sAdd(-1);
    //window.alert("calcDecryptExponent: p-1 = " + pm1.toString() );
    accumulate_output("p-1 =" + pm1.toString());
    var qm1 = q.sAdd(-1);
    //window.alert("calcDecryptExponent: q-1 = " + qm1.toString() );
    accumulate_output("(p-1)(q-1)=" + qm1.toString());
    //window.alert("calcDecryptExponent: calculating p-1*q-1");	  
    var p1q1 = pm1.mul(qm1);
    //window.alert( "calcDecryptExponent: (p-1)(q-1)=" + p1q1.toString() );
    accumulate_output("(p-1)(q-1)=" + p1q1.toString());

    // now start searching for a valid exponent
    while (true) {
        //window.alert( "calcDecryptExponent: trying e="+e.toString() );
        accumulate_output("trying e=" + e.toString());
        // try this exponent
        d = check_exponent(p1q1, e);

        // if we succeeded, bail out
        if (d != null) {
            // update the encryption exponent
            document.stuff.expe.value = e.toString();
            // return our decryption exponent
            //window.alert("calcDecryptExponent: check_exponent succeeded, returning");			
            return d;
        }
        //window.alert("calcDecryptExponent: check_exponent failed, trying next e" );
        // try the next value for e
        e = e.sAdd(2);
    }
}

// given a pair of primes, generate the exponents
function key_gen() {
    // reset debug/detail output
    accumulated_output_info = "";

    // get the two primes, send to the exponent generation code
    var p = new bigint();
    p.parseInt(document.stuff.primep.value);
    accumulate_output("p=" + p.toString());

    var q = new bigint();
    q.parseInt(document.stuff.primeq.value);
    accumulate_output("q=" + q.toString());

    // get the tentative encryption exponent
    // fix it at 17 by default
    var e = new bigint();
    //e.parseInt( document.stuff.expe.value );
    e.parseInt("17");
    accumulate_output("e=" + e.toString());

    //window.alert("calling calcDecryptExp");
    // get other values
    var d = calcDecryptExp(p, q, e);
    accumulate_output("d=" + d.toString());

    var n = p.mul(q);
    accumulate_output("n=" + n.toString());

    // save values for user
    //document.stuff.expe.value = e.toString();
    document.stuff.expd.value = d.toString();
    document.stuff.modulus.value = n.toString();

    // note how we got here
    document.stuff.details.value = accumulated_output_info;
}

// resulting message
var RSA_output = new bigint();

// do the actual RSA calculations
// encrypt using the normal modular exponentiation
function do_encrypt() {
    // reset debug/detail output
    accumulated_output_info = "";

    // what should we encrypt/decrypt?
    var inmsg = get_message(document.stuff.intype, document.stuff.indata.value);
    accumulate_output("message=" + inmsg.toString());
    //window.alert("encrypt: message=" + inmsg.toString() );

    // get the encryption exponent
    var e = new bigint();
    e.parseInt(document.stuff.expe.value);
    accumulate_output("e=" + e.toString());
    //window.alert("encrypt: e=" + e.toString() );

    var n = new bigint();
    n.parseInt(document.stuff.modulus.value);
    accumulate_output("n=" + n.toString());
    //window.alert("encrypt: n=" + n.toString() );

    // paranoia check
    if (inmsg.cmp(n) >= 0) {
        // message is larger than the modulus
        window.alert("Error: Message is larger than n");
        return;
    }

    // do the encryption
    //window.alert("encrypt: calling expMod");
    RSA_output = inmsg.expMod(e, n, true);
    //window.alert("encrypt: result=" + RSA_output.toString() );
    accumulate_output("result=" + RSA_output.toString());

    // process output
    format_output(document.stuff.outtype, RSA_output);

    // note how we got here
    document.stuff.details.value = accumulated_output_info;
}

// decrypt the string using the Chinese Remainder Theorem
function do_decryptCRT() {
    // reset debug/detail output
    accumulated_output_info = "";

    // what should we encrypt/decrypt?
    accumulate_output("Decrypting using the Chinese Remainder Theorem");
    var inmsg = get_message(document.stuff.intype, document.stuff.indata.value);
    accumulate_output("message=" + inmsg.toString());
    //window.alert("decrypt: message=" + inmsg.toString() );

    var n = new bigint();
    n.parseInt(document.stuff.modulus.value);
    accumulate_output("n=" + n.toString());
    //window.alert("encrypt: n=" + n.toString() );

    // paranoia check
    if (inmsg.cmp(n) >= 0) {
        // message is larger than the modulus
        //window.alert( "Error: Message is larger than n" );
        return;
    }

    var p = new bigint();
    p.parseInt(document.stuff.primep.value);
    accumulate_output("p=" + p.toString());

    var q = new bigint();
    q.parseInt(document.stuff.primeq.value);
    accumulate_output("q=" + q.toString());

    var d = new bigint();
    d.parseInt(document.stuff.expd.value);
    accumulate_output("d=" + d.toString());

    // these next values would normally be calculated in advance
    var d1 = d.mod(p.sAdd(-1));
    accumulate_output("d1 (d mod p-1) =" + d1.toString());
    //window.alert( "d1 (d mod p-1) =" + d1.toString() );

    var d2 = d.mod(q.sAdd(-1));
    accumulate_output("d2 (d mod q-1) =" + d2.toString());
    //window.alert( "d2 (d mod q-1) =" + d2.toString() );

    // calculate inverses
    var pinv = check_exponent(q, p);
    accumulate_output("p inverse =" + pinv.toString());
    //window.alert( "p inverse = " + pinv.toString() );

    var qinv = check_exponent(p, q);
    accumulate_output("q inverse =" + qinv.toString());
    //window.alert( "q inverse = " + qinv.toString() );

    // decrypt based on p and q
    accumulate_output("Calculating msg_p = msg**d1 mod p");
    var msg_p = inmsg.expMod(d1, p, true);
    accumulate_output("result=" + msg_p.toString());
    //window.alert( "msg_p = msg**d1 mod p" + msg_p.toString() );

    accumulate_output("Calculating msg_q = msg**d2 mod q");
    var msg_q = inmsg.expMod(d2, q, true);
    accumulate_output("result=" + msg_q.toString());
    //window.alert("msg_q = msg**d2 mod q" + msg_q.toString() );

    // get first part
    var part1 = msg_p.mul(qinv).mul(q);
    accumulate_output("msg_p * q inverse * q = " + part1.toString());
    //window.alert( "msg_p * q inverse * q = " + part1.toString() );
    // get second part
    var part2 = msg_q.mul(pinv).mul(p);
    accumulate_output("msg_q * p inverse * p = " + part2.toString());
    //window.alert( "msg_q * p inverse * p = " + part2.toString() );

    // compute final result
    RSA_output = part1.add(part2).mod(n);
    accumulate_output("decrypted message=" + RSA_output.toString());
    //window.alert( "decrypted message=" + RSA_output.toString() );

    // process output
    format_output(document.stuff.outtype, RSA_output);

    // note how we got here
    document.stuff.details.value = accumulated_output_info;
}

// do/change output formatting
function format_output() {
    // reprocess output
    document.stuff.outdata.value =
         format_bigint(document.stuff.outtype, RSA_output);
}