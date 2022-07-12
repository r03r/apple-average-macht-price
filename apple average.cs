//Update 08-24-2021
//Thanks for @sktennis for help with code
//Plots 8 Different types of Moving Averages
//Added Ability to Plot 1 or 2 Moving Averages - Fast MA & Slow MA
//Added Abaility to Plot Fast MA with Multi TimeFrame
//Added Abaility to Plot Slow MA with Multi TimeFrame
//Added Ability to Color Fast MA Based on Slope of MA
//Added Ability to Color Fast MA based on being Above/Below Slow MA
//Added Ability to Plot 8 Types of Moving Averages
//Simple, Exponential, Weighted, Hull, VWMA, RMA, TEMA, & Tilson T3
//Added Ability to Set Alerts Based on:
//Slope Change in the Fast MA Or Fast MA Crossing Above/Below Slow MA.
//Added Ability to Plot "Fill" if Both Moving Averages are Turned ON
//Added Ability to control Transparency of Fill

//Update 12-07-2021
//Converted to V5 PineScript

// This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
// © ChrisMoody

//@version=5
indicator(title="_CM_Ultimate_MA_MTF_V4", shorttitle="_CM_Ult_MA_MTF_V4", overlay=true)
//inputs
//Fast MA Inputs
res1                = input.timeframe("",  "Fast MA TimeFrame", group="Fast MA")
fast_ma_len         = input.int(defval=20, minval=1, maxval=999, title="Fast MA Length",  group="Fast MA")
src1                = input.source(defval=close, title="Fast MA Source", group="Fast MA")
fast_ma_off         = input.int(defval=0, minval=-200, maxval=200, title="Fast MA Offset",  group="Fast MA")
fast_ma_source      = input.string(defval="SMA", title="Fast MA Type", options=["SMA", "EMA", "WMA", "Hull MA", "VWMA", "RMA", "TEMA", "TIL T3"], group="Fast MA")
fast_factorT3       = input.float(defval=.7, title="Fast MA Tilson T3 Factor", minval=0, maxval=99, step = .1, tooltip="Default = .7", group="Fast MA")
//Slow MA Inputs
show_slow_ma        = input.bool(false,title="Show Slow MA?", group="Slow MA")
res2                = input.timeframe("",  "Slow MA TimeFrame", group="Slow MA")
slow_ma_len         = input.int(defval=50, minval=1, maxval=999, title="Slow MA Length",  group="Slow MA")
src2                = input.source(defval=close, title="Slow MA Source", group="Slow MA")
slow_ma_off         = input.int(defval=0, minval=-200, maxval=200, title="Slow MA Offset",  group="Slow MA")
slow_ma_source      = input.string(defval="SMA", title="Slow MA Type", options=["SMA", "EMA", "WMA", "Hull MA", "VWMA", "RMA", "TEMA", "TIL T3"], group="Slow MA")
slow_factorT3       = input.float(defval=.7, title="Slow MA Tilson T3 Factor", minval=0, maxval=99, step= .1, tooltip="Default = .7", group="Slow MA")
//Plot Settings
fast_ma_col         = input.color(#2962FF, title="Fast MA  ", group="Plot Settings", inline="P10")
slow_ma_col         = input.color(#FF6D00, title="Slow MA  ", group="Plot Settings", inline="P10")
fast_ma_wid         = input.int(defval=3, minval=0, maxval=5, title="Fast MA Width",  group="Plot Settings")
slow_ma_wid         = input.int(defval=3, minval=0, maxval=5, title="Slow MA Width",  group="Plot Settings")
show_fast_ma_cc     = input.bool(false, title="Change Fast MA Color Based On Slope?", group="Plot Settings")
fast_ma_smooth      = input.int(defval=2, minval=1, maxval=20, title="Color Smoothing", tooltip="Setting 1 = No Smoothing", group="Plot Settings")
bullish_col         = input.color(color.new(color.green, 0), "Bull Col  ", group="Plot Settings", inline="P40")
bearish_col         = input.color(color.new(color.red, 0),   "Bear Col  ", group="Plot Settings", inline="P40")
show_fast_ma_tr     = input.bool(false, title="Change Fast MA Color Above/Below Slow MA?", group="Plot Settings")
show_fill           = input.bool(true, title="Show Fill?", group="Plot Settings", inline="P50")
fill_transp         = input.int(defval=50, title="Transparency", minval=0, maxval=100, group="Plot Settings", inline="P60")
// Alerts T/F Inputs
alert_Slope_Bull    = input.bool(false, title = "Fast MA Slope Bull", group = "Alerts", inline="Alert10")
alert_Slope_Bear    = input.bool(false, title = "Fast MA Slope Bear", group = "Alerts", inline="Alert10")
alert_Cross_Bull    = input.bool(false, title = "Fast MA Cross Bull", group = "Alerts", inline="Alert20")
alert_Cross_Bear    = input.bool(false, title = "Fast MA Cross Bear", group = "Alerts", inline="Alert20")

//Begin Coding Calcs & Definitions
//Fast MA Calcs
//Hull MA Definition
hullma = ta.wma(2*ta.wma(src1, fast_ma_len/2)-ta.wma(src1, fast_ma_len), math.round(math.sqrt(fast_ma_len)))
//TEMA definition
ema1 = ta.ema(src1, fast_ma_len)
ema2 = ta.ema(ema1, fast_ma_len)
ema3 = ta.ema(ema2, fast_ma_len)
tema = 3 * (ema1 - ema2) + ema3
//Tilson T3
factor = fast_factorT3 * 1 //.10
gd(src1, fast_ma_len, factor) => ta.ema(src1, fast_ma_len) * (1 + factor) - ta.ema(ta.ema(src1, fast_ma_len), fast_ma_len) * factor 
t3(src1, fast_ma_len, factor) => gd(gd(gd(src1, fast_ma_len, factor), fast_ma_len, factor), fast_ma_len, factor) 
tilT3 = t3(src1, fast_ma_len, factor)
//Fast MA mapped to fast_ma_source
fast_ma = request.security(syminfo.tickerid, res1, fast_ma_source == "SMA" ? ta.sma(src1, fast_ma_len) : 
  fast_ma_source == "EMA" ? ta.ema(src1, fast_ma_len) : 
  fast_ma_source == "WMA" ? ta.wma(src1, fast_ma_len) : 
  fast_ma_source == "Hull MA" ? hullma : 
  fast_ma_source == "VWMA" ? ta.vwma(src1, fast_ma_len) : 
  fast_ma_source == "RMA" ? ta.rma(src1, fast_ma_len) : 
  fast_ma_source == "TEMA" ? tema : tilT3)
//Slow MA Calcs
//Hull MA Definition for 2nd MA
hullma_Slow = ta.wma(2*ta.wma(src2, slow_ma_len/2)-ta.wma(src2, slow_ma_len), math.round(math.sqrt(slow_ma_len)))
//TEMA definition for 2nd MA
ema1_Slow = ta.ema(src2, slow_ma_len)
ema2_Slow = ta.ema(ema1_Slow, slow_ma_len)
ema3_Slow = ta.ema(ema2_Slow, slow_ma_len)
tema_Slow = 3 * (ema1_Slow - ema2_Slow) + ema3_Slow
//Tilson T3 for 2nd MA
factor_Slow = slow_factorT3 * 1 //.10
gd_Slow(src2, slow_ma_len, factor_Slow) => ta.ema(src2, slow_ma_len) * (1 + factor_Slow) - ta.ema(ta.ema(src2, slow_ma_len), slow_ma_len) * factor_Slow 
t3_Slow(src2, slow_ma_len, factor_Slow) => gd_Slow(gd_Slow(gd_Slow(src2, slow_ma_len, factor_Slow), slow_ma_len, factor_Slow), slow_ma_len, factor_Slow) 
tilT3_Slow = t3_Slow(src2, slow_ma_len, factor_Slow)
//Slow MA mapped to slow_ma_source for 2nd MA
slow_ma = request.security(syminfo.tickerid, res2, slow_ma_source == "SMA" ? ta.sma(src2, slow_ma_len) : 
  slow_ma_source == "EMA" ? ta.ema(src2, slow_ma_len) : 
  slow_ma_source == "WMA" ? ta.wma(src2, slow_ma_len) : 
  slow_ma_source == "Hull MA" ? hullma_Slow : 
  slow_ma_source == "VWMA" ? ta.vwma(src2, slow_ma_len) : 
  slow_ma_source == "RMA" ? ta.rma(src2, slow_ma_len) : 
  slow_ma_source == "TEMA" ? tema_Slow : tilT3_Slow)

//Color Change for Fast MA based on Slope
fast_ma_up     = fast_ma >= fast_ma[fast_ma_smooth]
fast_ma_dn     = fast_ma  < fast_ma[fast_ma_smooth]
col_fast_ma_cc = show_fast_ma_cc and fast_ma_up ? bullish_col : show_fast_ma_cc and fast_ma_dn ? bearish_col : color.new(fast_ma_col, 0)
//Color Change for Fast MA Above/Below Slow MA
fast_Above_slow = fast_ma >= slow_ma
fast_Below_slow = fast_ma <  slow_ma
col_fast_Above_Below = show_fast_ma_tr and fast_Above_slow ? bullish_col : show_fast_ma_tr and fast_Below_slow ? bearish_col : color.new(fast_ma_col, 0)
//Color Change Condition Final
col_Final = show_fast_ma_tr ? col_fast_Above_Below : col_fast_ma_cc

//Begin Plot Statements
plot(fast_ma, title="Fast MA",  color=color.new(col_Final, 0), style=plot.style_line, linewidth=fast_ma_wid, offset=fast_ma_off)
plot(show_slow_ma and slow_ma ? slow_ma : na, title="Slow MA",  color=color.new(slow_ma_col, 0), style=plot.style_line, linewidth=slow_ma_wid, offset=slow_ma_off)
//Plot Statements if Fill is ON
p1 = plot(show_fill and fast_ma ? fast_ma : na, title="Fast MA",  color=color.new(col_Final, 0), style=plot.style_line, linewidth=fast_ma_wid, offset=fast_ma_off, editable=false)
p2 = plot(show_fill and show_slow_ma and slow_ma ? slow_ma : na, title="Slow MA",  color=color.new(slow_ma_col, 0), style=plot.style_line, linewidth=slow_ma_wid, offset=slow_ma_off, editable=false)
fill(p1, p2, title="1", color=color.new(col_Final,fill_transp))

//Alert Conditions
//Alert Conditions Based on Sloe of Fast MA
alert_Fast_MA_Slope_UP = fast_ma[1] < fast_ma[1 + fast_ma_smooth] and fast_ma[0] >= fast_ma[0 + fast_ma_smooth]
alert_Fast_MA_Slope_DN = fast_ma[1] > fast_ma[1 + fast_ma_smooth] and fast_ma[0] <  fast_ma[0 + fast_ma_smooth]
//Alert Conditions when Fast MA Crosses Above/Below Slow MA
alert_Cross_UP = fast_ma[1] < slow_ma[1] and fast_ma >= slow_ma
alert_Cross_DN = fast_ma[1] > slow_ma[1] and fast_ma <  slow_ma

//Alerts
//Alerts Based on Slope of Fast MA
if alert_Slope_Bull and alert_Fast_MA_Slope_UP
    alert("Symbol = (" + syminfo.tickerid + ") Fast MA TimeFrame = (" + res1 + ") Current Price (" + str.tostring(close) + ") Fast MA Slope Up.", alert.freq_once_per_bar_close)
if alert_Slope_Bear and alert_Fast_MA_Slope_DN
    alert("Symbol = (" + syminfo.tickerid + ") Fast MA TimeFrame = (" + res1 + ") Current Price (" + str.tostring(close) + ") Fast MA Slope Down.", alert.freq_once_per_bar_close)
//Alerts Based on Fast MA Crossing Above/Below Slow MA
if alert_Cross_Bull and alert_Cross_UP
    alert("Symbol = (" + syminfo.tickerid + ") Fast MA TimeFrame = (" + res1 + ") Slow MA TimeFrame = (" + res2 + ") Current Price (" + str.tostring(close) + ") Fast MA Crosses Above Slow MA.", alert.freq_once_per_bar_close)
if alert_Cross_Bear and alert_Cross_DN
    alert("Symbol = (" + syminfo.tickerid + ") Fast MA TimeFrame = (" + res1 + ") Slow MA TimeFrame = (" + res2 + ") Current Price (" + str.tostring(close) + ") Fast MA Crosses Below Slow MA.", alert.freq_once_per_bar_close)

//End Code