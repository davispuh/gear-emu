#!/usr/bin/env bash
#Update Date value in copyright area on headers of .cs files

set -o pipefail

#===============================================
#global configuration

#default file pattern to use
declare -g filePatt="*.cs"
#directories to search on
declare -g targetDirs
declare -g -a defaultTargetDirs=("Gear" "Gear/Disassembler" "Gear/EmulationCore" "Gear/GUI" "Gear/GUI/LogicProbe" "Gear/PluginSupport" "Gear/Propeller" "Gear/Utils")
declare -g customTargetDirs=false

#===============================================
#Option declarations
declare -i -r opSummaryLineEndings=1
declare -i -r opUpdate2WinLineEndings=2
declare -i -r opUpdate2LinLineEndings=3
declare -i -r opSummaryExtraSpaces=4
declare -i -r opDeleteExtraSpaces=5
declare -i -r opExit=6

declare -a Options
Options[$opSummaryLineEndings]="Show summary of Line endings"
Options[$opUpdate2WinLineEndings]="Update Line endings to windows"
Options[$opUpdate2LinLineEndings]="Update Line endings to unix"
Options[$opSummaryExtraSpaces]="Show summary of extra spaces on line ends"
Options[$opDeleteExtraSpaces]="Delete extra spaces on line ends"
Options[$opExit]="Exit"

#===============================================
#global variables
#Version of this program
declare -r version='1.0'
#Program name without path
declare -g progName=${0##*/}
#Program description
declare -r programDescription="Utils for show/modify characteristics of \"$filePatt\" files for Gear project."

declare -g retVal

declare -g destFiles=""

declare basePathThisProgram=$(
   cd -- "$(dirname "$0")" >/dev/null 2>&1 || exit
   pwd -P
)
declare basePathOrig=$basePathThisProgram
#Base path for tree structure
declare -g basePath=${basePathThisProgram%/*}

#Show progress bar
declare showProgress
#Hide messages of Line converter
declare quietLineConvert

#===============================================
#Set global error level tags
declare -r -i err_OptionNotRecognized=1
declare -r -i err_MissingValue=2
declare -r -i err_InternalError=3
declare -r -i err_FileNotFound=4
declare -r -i err_IncompatibleOption=5

#Definitions for format and colors
if which tput >/dev/null 2>&1 && [[ $(tput -T"$TERM" colors) -ge 8 ]]; then
   declare -x fmtBold="\e[1m"
   declare -x fmtUnd="\e[4m"
   declare -x fmtInv="\e[7m"
   declare -x fmtWarn="\e[33m"
   declare -x fmtUser="\e[94m"
   declare -x fmtErr="\e[31m"
   declare -x fmtOk="\e[32m"
   declare -x fmtReset="\e[0m"
   declare -x fmtSection="\e[1;44;33m"
fi

#===============================================
#Functions

#Expand a file or directory to full path
#  $1 : file or directory to expand
# Supported patterns:
#   ~/ | ~/Gear/       - home shortcut to directory
#   ../Gear/ | ./Gear/ - relative path to directory
#   /home/user/Gear/   - absolute path to directory
#   ../readme.md | ./readme.md     - relative path to file
#   ~/readme.md | ~/Gear/readme.md - home shortcut to file
#   /home/user/Gear/readme.md      - absolute path to file
function Expand2AbsolutePath() {
   local base
   local file
   local useTmp=false
   local par=${1//\'/}
   #search for root dir
   if [[ $par == / ]]; then
      echo "/"
      return
   fi
   #search for file starting with /
   if [[ $par =~ ^/[^/]+$ ]]; then
      echo $par
      return
   fi
   #replace ~ for homedir
   if [[ $1 =~ ^~ ]]; then
      par=${par//\~/$(eval echo "~$USER")}
   fi
   #search for ending with /
   if [[ $par =~ ^.*/$ ]]; then
      base=${par}tmp
      useTmp=true
   else
      base=$par
      file=$(basename $par)
   fi
   base=$(cd -- "$(dirname "$base")" >/dev/null 2>&1 || return; pwd -P)
   if [[ -z $base ]]; then
      return 1
   fi
   if $useTmp; then
      echo "${base}/"
   else
      echo "${base}/${file}"
   fi
}

#Show usage
function ShowUsage() {
   #generate the output
   cat <<EOF >&2
Usage:
  $progName [-b DIR [ -n | -l ] ] [ -e EXT[,EXT]... ] [ -p ] [FILE ...]
  $progName [ -l | -h | -v ]

  DIR : Base directory
  FILE: Aditional file(s) or directories to process
  EXT : Extension(s) of files to process, separated by commas ','.

Options:
  -e | --extension        Specify extension(s) to look for, replacing default: '$filePatt'.
  -b | --base-dir         Specify base directory, replacing default: '$basePath'.
  -n | --no-predefined    Do not use predefined dirs.
  -p | --show-progress    Show progress bar.
  -l | --list-predefined  Print a list of predefined dirs.
  -h | --help             Show usage.
  -v | --version          Show program version.
EOF
}

#Process parameters for main
# Exit error level
#   err_OptionNotRecognized
#   err_MissingValue
#   err_FileNotFound
#   err_IncompatibleOption
function ProcessParams() {
   #define script options
   local options='e:b:nplhv'
   local longopts='extension:,base-dir:,no-predefined,showProgress,list-predefined,help,version'
   ! parsed=$(getopt --options=$options --longoptions=$longopts --name "$progName" -- "$@")
   if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
      ShowUsage
      exit $err_OptionNotRecognized
   fi
   # read getopt’s output this way to handle the quoting right, setting value to $1.
   eval set -- "$parsed"
   while true; do
      case $1 in
      -e | --extensions)
         filePatt=${2//,/ }
         if [[ -z $filePatt || $filePatt == -- ]]; then
            echo -e "${fmtErr}Extension is empty for parameter $1. Aborting.${fmtReset}"
            ShowUsage
            exit $err_MissingValue
         fi
         shift 2
         ;;
      -b | --base-dir)
         basePath=$(Expand2AbsolutePath "$2")
         if [[ -z $basePath || $basePath == -- ]]; then
            echo -e "${fmtErr}Base directory is empty for parameter $1. Aborting.${fmtReset}"
            ShowUsage
            exit $err_MissingValue
         elif [[ ! -d $basePath ]]; then
            echo -e "${fmtErr}Base directory not reached: '$2'. Aborting.${fmtReset}"
            ShowUsage
            exit $err_FileNotFound
         else
            basePathOrig=$basePath
            basePath=${basePath%/*}
            shift 2
         fi
         ;;
      -n | --no-predefined)
         customTargetDirs=true
         shift
         ;;
      -p | --show-progress)
         showProgress="--bar"
         quietLineConvert="--quiet"
         shift
         ;;
      -l | --list-predefined)
         if [[ -z $targetDirs ]]; then
            echo -e "${fmtErr}Incompatible option '-n | --no-predefined' used.${fmtReset}"
            ShowUsage
            exit $err_IncompatibleOption
         else
            for dir in $targetDirs; do
               echo "${basePath}/${dir}/"
            done
            exit
         fi
         shift
         ;;
      -h | --help)
         echo -e "${fmtBold}${programDescription}${fmtReset}"
         ShowUsage
         exit
         ;;
      -v | --version)
         echo "Version $version"
         exit
         ;;
      --)
         shift
         break
         ;;
      *)
         echo -e "${fmtErr}Option not recognized: '$1'.${fmtReset}" >&2
         ShowUsage
         exit $err_OptionNotRecognized
         ;;
      esac
   done
   #get full path on targetDirs
   if ! $customTargetDirs; then
      for (( i = 0; i < ${#defaultTargetDirs[@]}; i++ )); do
         targetDirs+="$basePath/${defaultTargetDirs[i]} "
      done
   else
      targetDirs+=$basePath
   fi
   #read positional parameter(s)
   while [[ $# -gt 0 ]]; do
      if [[ -e $1 ]]; then
         if [[ -d $1 ]]; then
            targetDirs+="$(Expand2AbsolutePath "$1") "
         elif [[ -f $1 ]]; then
            destFiles+="$1 "
         fi
      else
         echo -e "${fmtWarn}File not reached: '$1'. It will be ignored.${fmtReset}" >&2
      fi
      shift
   done
}

#
function GenerateCommand() {
   local cmd cmd1 cmd2
   if [[ -n $destFiles ]]; then
      cmd1="stdbuf --output=L echo ${destFiles}"
      cmd=$cmd1
   fi
   if [[ -n $targetDirs ]]; then
      cmd2="parallel $showProgress --line-buffer find \"{1}\" -maxdepth 1 -type f -name \"{2}\" -print ::: $targetDirs ::: $filePatt"
      cmd=$cmd2
   fi
   if [[ -n $cmd1 && -n $cmd2 ]]; then
         cmd="{ $cmd1; $cmd2; } "
   elif [[ -z $cmd ]]; then
      echo -e "${fmtErr}Contradictory options lead to have nothing to do. Aborting.${fmtReset}" >&2
      return 1
   fi
   retVal=$cmd
   return 0
}

#Main
#  $@ : all the parameters of the program
# Exit error level
#   err_IncompatibleOption
function Main() {
   ProcessParams "$@"
   echo -e "${fmtBold}${progName} - ${programDescription}${fmtReset}"
   local mustSelectOption=true
   local mustExec
   retVal=""
   while $mustSelectOption; do
      select option in "${Options[@]}"; do
         mustExec=true
         if [[ -n $option ]]; then
            echo -e "${fmtInv}${fmtOk}${option}${fmtReset}${fmtOk} - base dir: '$basePath'${fmtReset}" >&2
         fi
         while $mustExec; do
            case $REPLY in
            "$opSummaryLineEndings")
               if [[ -z $retVal ]]; then
                  if ! GenerateCommand; then
                     exit $err_IncompatibleOption
                  fi
               fi
               eval "$retVal" | #emit list of files
               parallel $showProgress --tag --tagstring "{/}:" --line-buffer dos2unix -idubtp {} |
               parallel --line-buffer --cat sed -E -e 's/\\s\{2,\}/\\t/g' {} | #replace 2+ spaces to tab
               parallel --line-buffer --cat cut -f-5 {} | #preserve 5 firsts fields
               stdbuf --output=L sort |
               stdbuf --output=L cat --number-nonblank | #number the columns
               stdbuf --output=L cat <(echo -e "${fmtBold}${fmtUnd}Number\tFile\tWin ending\tLinux ending\tByte order\tType\t${fmtReset} ") - |
               stdbuf --output=L column -s '	' -t #format columns
               mustExec=false
               mustSelectOption=false
               break
               ;;
            "$opUpdate2WinLineEndings")
               if [[ -z $retVal ]]; then
                  if ! GenerateCommand; then
                     exit $err_IncompatibleOption
                  fi
               fi
               if eval "$retVal" |
               parallel $showProgress --tag --tagstring "{/}:" --line-buffer unix2dos $quietLineConvert --keep-bom -ascii --oldfile {}; then
                  echo -e "${fmtOk}New values:${fmtReset}\n"
                  REPLY=$opSummaryLineEndings
               else
                  mustExec=false
                  mustSelectOption=false
                  break
               fi
               ;;
            "$opUpdate2LinLineEndings")
               if [[ -z $retVal ]]; then
                  if ! GenerateCommand; then
                     exit $err_IncompatibleOption
                  fi
               fi
               if eval "$retVal" |
               parallel $showProgress --tag --tagstring "{/}:" --line-buffer dos2unix $quietLineConvert --keep-bom -ascii --oldfile {}; then
                  echo -e "${fmtOk}New values:${fmtReset}\n"
                  REPLY=$opSummaryLineEndings
               else
                  mustExec=false
                  mustSelectOption=false
                  break
               fi
               ;;
            "$opSummaryExtraSpaces")
               if [[ -z $retVal ]]; then
                  if ! GenerateCommand; then
                     exit $err_IncompatibleOption
                  fi
               fi
               eval "$retVal" |
               parallel $showProgress --tag --tagstring "{/}:" --line-buffer grep --count --text -E -e \'[[:space:]]+$\' {} |
               stdbuf --output=L sort |
               stdbuf --output=L cat --number-nonblank |
               stdbuf --output=L cat <(echo -e "${fmtBold}${fmtUnd}Number\tFile\tLines with extra spaces\t${fmtReset} ") - |
               stdbuf --output=L column -s '	' -t
               mustExec=false
               mustSelectOption=false
               break
               ;;
            "$opDeleteExtraSpaces")
               if [[ -z $retVal ]]; then
                  if ! GenerateCommand; then
                     exit $err_IncompatibleOption
                  fi
               fi
               if eval "$retVal" |
                  parallel $showProgress --line-buffer sed -E -i -e \'s/[[:space:]]+$//\' {}; then
                  echo -e "${fmtOk}New values:${fmtReset}\n"
                  REPLY=$opSummaryExtraSpaces
               else
                  mustExec=false
                  mustSelectOption=false
                  break
               fi
               ;;
            "$opExit")
               mustExec=false
               mustSelectOption=false
               break
               ;;
            *)
               echo -e "${fmtWarn}'$REPLY' is not a valid option!${fmtReset}"
               mustExec=false
               option=""
               break
            esac
         done
      if ! $mustSelectOption; then
         break
      fi
      done
   done
}

#Main invocation
Main "$@"
